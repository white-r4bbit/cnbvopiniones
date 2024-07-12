using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Cnbv.ConectaProcesos.Opiniones.Common;
using Cnbv.ConectaProcesos.Opiniones.Data.DatabaseConectaProcesos;
using Cnbv.ConectaProcesos.Opiniones.Data.Interfaces;
using Cnbv.ConectaProcesos.Opiniones.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Cnbv.ConectaProcesos.Opiniones.Business
{
  public class OpinionesBusiness : IOpinionesBusiness
  {
    // Para conectarse a cada una de las tablas de la base de datos.
    private readonly IRepository<Opinion> _opinionRepository;
    private readonly IRepository<OpinionReceptor> _opinionReceptorRepository;
    private readonly IRepository<TipoElementoEnum> _elementosRepository;
    private readonly IRepository<TipoDocumentoEnum> _documentosRepository;
    private readonly IRepository<ArchivoOpinion> _archivoOpinionRepository;

    private readonly KeyVaultService _keyVaultService;

    public OpinionesBusiness(IRepository<Opinion> opinionRepository, IRepository<TipoElementoEnum> elementosRepository,
                              IRepository<TipoDocumentoEnum> documentosRepository, IRepository<OpinionReceptor> opinionReceptorRepository, KeyVaultService keyVaultService,
                              IRepository<ArchivoOpinion> archivoOpinionRepository)
    {
      _opinionRepository = opinionRepository;
      _opinionReceptorRepository = opinionReceptorRepository;
      _elementosRepository = elementosRepository;
      _documentosRepository = documentosRepository;
      _keyVaultService = keyVaultService;
      _archivoOpinionRepository = archivoOpinionRepository;
    }

    /// <summary>
    /// Operación para guardar la información de una opinión solicitada por un usuario.
    /// </summary>
    /// <param name="solicitud"> DTO que guarda la información necesaria para registrar una opinión. </param>
    /// <returns> Code response y string (identificador o mensaje con el mensaje de error). </returns>
    public async Task<int> SolicitarOpinionesAsync(SolicitarOpiniones solicitud)
    {
      // Verificar que el folio que se está solicitando no se encuentra activo.
      var opinionesActivas = await _opinionRepository.GetFilteredAsync(o => o.FolioAsunto == solicitud.FolioAsunto && o.Activa);
      if (opinionesActivas.Any())
      {
        throw new InvalidOperationException("Existe un proceso de opinión previo que no ha sido finalizado.");
      }
      else
      {
        //Inicializamos la función BeginTransaction para asegurar la integridad de la modificación de la base de datos a lo largo de las operaciones.
        using (var transaction = await _opinionRepository.BeginTransactionAsync())
        {
          try
          {
            //// Se genera el objeto opinión y se integran sus propiedades.
            Opinion opinion = await GenerarNuevaOpinionAsync(solicitud);
            await _opinionRepository.AddAsync(opinion);
            // Se guarda la transacción.
            await transaction.CommitAsync();
            return opinion.Id;
          }
          catch (InvalidOperationException)
          {
            throw;
          }
          catch
          {
            // En caso de que haya ocurridó un error, los cambios echos a la base de datos se deshacen.
            await transaction.RollbackAsync();
            throw new DbUpdateException("Ocurrió un error al intentar almacenar la información de la opinión.");
          }
        }
      }
    }


    /// <summary>
    /// Operación para mandar a traer todas las opiniones asociadas a un folio.
    /// </summary>
    /// <param name="folio"> Folio asignado a la opinión. </param>
    /// <returns> Lista de las opiniones asociadas al folio que dió el usuario. </returns>
    public async Task<List<ObtenerOpinionesModel>> ObtenerOpinionesAsync(string folio)
    {
      try
      {
        IEnumerable<Opinion> opiniones = new List<Opinion>();
        List<ObtenerOpinionesModel> opinionesResponse = new List<ObtenerOpinionesModel>();
        // Traer todos las opiniones asociadas al folio que envío el usuario. 
        opiniones = await _opinionRepository.GetFilteredAsync(o => o.FolioAsunto.Equals(folio));

        opiniones = opiniones.OrderByDescending(o => o.FechaSolicitud);

        if (opiniones != null && opiniones.Count() > 0)
        {
          foreach (var o in opiniones)
          {
            ObtenerOpinionesModel opinionResponse = new ObtenerOpinionesModel();
            opinionResponse.Identificador = o.Id;
            IEnumerable<OpinionReceptor> receptores = await _opinionReceptorRepository.GetFilteredAsync(i => i.IdOpinion == o.Id);

            receptores = receptores.OrderByDescending(r => r.FechaRespuesta);

            // Llenado de información de opiniones.
            opinionResponse.FolioAsunto = o.FolioAsunto;
            opinionResponse.FechaSolicitud = o.FechaSolicitud;
            opinionResponse.EnProceso = o.Activa;
            int version = o.Version;
            opinionResponse.Version = version;
            List<ObtenerOpinionesReceptores> lista = new List<ObtenerOpinionesReceptores>();
            foreach (var r in receptores)
            {
              ObtenerOpinionesReceptores receptor = new ObtenerOpinionesReceptores();
              // Llenado de infoormación de receptores.
              receptor.Id = r.Id;
              receptor.Clave = r.Clave;
              receptor.Nombre = r.Nombre;
              receptor.EsInterna = r.Interno;
              receptor.FechaRespuesta = r.FechaRespuesta;
              receptor.Obligatoria = r.Obligatoria;
              receptor.EnProceso = r.Activa;
              receptor.IdEnvio = r.IdEnvio;
              receptor.Firmante = r.Firmante;
              receptor.EstatusSolicitud = r.EstatusSolicitud;
              receptor.ComentarioFirmante = r.ComentarioFirmante;

              lista.Add(receptor);
            }
            opinionResponse.Receptores = lista.ToArray();
            opinionesResponse.Add(opinionResponse);
          }

          return opinionesResponse;
        }
        else
        {
          throw new InvalidOperationException("No se encontró información con el folio solicitado");
        }
      }
      catch
      {
        throw;
      }
    }


    /// <summary>
    /// Operación para traer más información sobre una opinión en especial.
    /// </summary>
    /// <param name="idOpinionReceptor"> id de la opinion de cada receptor (id asignado en la tabla OpinionReceptor) </param>
    /// <returns> Detalle de la opinión asignada a un receptor. </returns>
    public async Task<ObtenerDetalleOpinion> ObtenerDetalleOpinion(int idOpinionReceptor)
    {
      try
      {
        // Traer las opiniones del receptor asociado al id dado por el usuario.
        IEnumerable<OpinionReceptor> opinionesReceptor = await _opinionReceptorRepository.GetFilteredAsync(i => i.Id == idOpinionReceptor);
        if (opinionesReceptor != null)
        {
          var opinionReceptor = opinionesReceptor.FirstOrDefault();
          if (opinionReceptor != null)
          {
            int idOpinion = opinionReceptor.IdOpinion;
            Opinion opinion = await _opinionRepository.GetByIdAsync(idOpinion);
            if (opinion != null)
            {
              // Comentarios opinión.
              string comentariosOpinion = opinion.Detalle;
              ObtenerDetalleOpinion detalleOpinion = new ObtenerDetalleOpinion();
              detalleOpinion.Comentarios = comentariosOpinion;
              detalleOpinion.Id = idOpinion;
              //Archivos asociados al detalle de la opinion.
              var archivosOpinion = await _archivoOpinionRepository.GetFilteredAsync(i => i.IdOpinion == idOpinion);
              archivosOpinion = archivosOpinion.OrderByDescending(a => a.FechaCreacion);
              archivosOpinion = archivosOpinion.Where(a => a.Eliminado == false);
              List<ArchivosModelResponse> archivosRespuesta = new List<ArchivosModelResponse>();
              foreach (var archivoOpinion in archivosOpinion)
              {
                ArchivosModelResponse archivoRespuesta = new ArchivosModelResponse();
                archivoRespuesta.Id = archivoOpinion.Id;
                archivoRespuesta.Ruta = archivoOpinion.Ruta ?? string.Empty;
                archivoRespuesta.FechaCreacion = archivoOpinion.FechaCreacion;
                archivoRespuesta.Nombre = archivoOpinion.Nombre;
                // Para obtener tipo Documento y tipo Elemento.
                int? idElemento = archivoOpinion.IdTipoElemento;
                int? idDocumento = archivoOpinion.IdTipoDocumento;
                // Traer el id del elemento y del documento según corresponda.
                TipoElementoEnum elementoName = await _elementosRepository.GetByConditionAsync(i => i.Id == idElemento);
                TipoDocumentoEnum documentoName = await _documentosRepository.GetByConditionAsync(i => i.Id == idDocumento);
                archivoRespuesta.TipoElemento = elementoName.Nombre;
                archivoRespuesta.TipoDocumento = documentoName.Nombre;
                archivosRespuesta.Add(archivoRespuesta);
              }
              detalleOpinion.Archivos = archivosRespuesta.ToArray();
              //Variable para guardar receptor asociado al detalle de la opinion.
              ObtenerDetalleOpinionReceptor receptorRespuesta = new ObtenerDetalleOpinionReceptor();
              receptorRespuesta.Id = opinionReceptor.Id;
              receptorRespuesta.Clave = opinionReceptor.Clave;
              receptorRespuesta.Nombre = opinionReceptor.Nombre;
              receptorRespuesta.Archivos = new List<ArchivosModelResponse>();
              receptorRespuesta.FinalizadaPor = opinionReceptor.FinalizadaPor;
              receptorRespuesta.Comentarios = opinionReceptor.Comentarios;
              receptorRespuesta.FechaRespuesta = opinionReceptor.FechaRespuesta;
              receptorRespuesta.IdEnvio = opinionReceptor.IdEnvio;
              receptorRespuesta.Firmante = opinionReceptor.Firmante;
              receptorRespuesta.EstatusSolicitud = opinionReceptor.EstatusSolicitud;
              receptorRespuesta.ComentarioFirmante = opinionReceptor.ComentarioFirmante;
              var archivosReceptor = await _archivoOpinionRepository.GetFilteredAsync(i => i.IdReceptor == idOpinionReceptor);
              archivosReceptor = archivosReceptor.Where(a => a.Eliminado == false);

              archivosReceptor = archivosReceptor.OrderByDescending(a => a.FechaCreacion);

              foreach (var archivoReceptor in archivosReceptor)
              {
                //Generar objeto para llenar la lista
                ArchivosModelResponse archivoReceptorRespuesta = new ArchivosModelResponse();
                archivoReceptorRespuesta.Id = archivoReceptor.Id;
                archivoReceptorRespuesta.Ruta = archivoReceptor.Ruta;
                archivoReceptorRespuesta.FechaCreacion = archivoReceptor.FechaCreacion;
                TipoElementoEnum elementoName = await _elementosRepository.GetByConditionAsync(i => i.Id == archivoReceptor.IdTipoElemento);
                TipoDocumentoEnum documentoName = await _documentosRepository.GetByConditionAsync(i => i.Id == archivoReceptor.IdTipoDocumento);
                archivoReceptorRespuesta.TipoElemento = elementoName.Nombre;
                archivoReceptorRespuesta.TipoDocumento = documentoName.Nombre;
                archivoReceptorRespuesta.Nombre = archivoReceptor.Nombre;
                receptorRespuesta.Archivos.Add(archivoReceptorRespuesta);
              }
              detalleOpinion.Receptor = receptorRespuesta;
              return detalleOpinion;
            }
            else
            {
              throw new InvalidOperationException("No se encontró información con el identificador proporcionado.");
            }
          }
          else
          {
            throw new InvalidOperationException("No se encontró informción con el identificador proporcionado");
          }
        }
        else
        {
          throw new InvalidOperationException("No se encontró información con el identificador proporcionado.");
        }
      }
      catch
      {
        throw;
      }
    }


    /// <summary>
    /// Función para subir los archivos del usuario a la base de datos.
    /// </summary>
    /// <param name="archivosRequest"> Información para almacenar un archivo. </param>
    /// <returns></returns>
    public async Task AgregarArchivos(ArchivosRequest archivosRequest)
    {
      var opinionIncluding = await _opinionRepository.GetByIdIncludingAsync(archivosRequest.Id, o => o.ArchivoOpinions);

      if (opinionIncluding != null)
      {
        using (var transaction = await _opinionRepository.BeginTransactionAsync())
        {
          try
          {
            foreach (var a in archivosRequest.Archivos)
            {
              var archivo = new ArchivoOpinion
              {
                Ruta = a.Ruta,
                FechaCreacion = a.FechaCreacion,
                Nombre = a.Nombre,
                IdOpinion = archivosRequest.Id,
                IdTipoElemento = (await _elementosRepository.GetByConditionAsync(t => t.Nombre == a.TipoElemento)).Id,
                IdTipoDocumento = (await _documentosRepository.GetByConditionAsync(t => t.Nombre == a.TipoDocumento)).Id,
              };

              opinionIncluding.ArchivoOpinions.Add(archivo);

            }
            await _opinionRepository.UpdateAsync(opinionIncluding);

            await transaction.CommitAsync();
          }
          catch
          {
            await transaction.RollbackAsync();
          }
        }
      }
      else
      {
        new InvalidOperationException("No se encontró la opinión con el identificador proporcionado.");
      }
    }


    /// <summary>
    /// Operación para guardar la respuesta y archivos que se envian como respuesta por parte del receptor
    /// </summary>
    /// <param name="finalizarOpinionRequest"> Parametros de la opinión que se quiere cerrar. </param>
    /// <returns></returns>
    public async Task FinalizarOpinion(FinalizarOpinion finalizarOpinionRequest)
    {
      string folioAsunto = string.Empty;
      bool updateAsunto = false;
      using (var transaction = await _opinionReceptorRepository.BeginTransactionAsync())
      {
        try
        {
          //Guardar comentarios en la tabla Opinion Receptor
          var opinionReceptor = await _opinionReceptorRepository.GetByIdIncludingAsync(finalizarOpinionRequest.IdOpinionReceptor, or => or.IdOpinionNavigation, or => or.ArchivoOpinions);
          if (opinionReceptor != null)
          {
            if (!opinionReceptor.Interno)
            {
              throw new InvalidOperationException("Para finalizar una opinión externa se debe utilizar el método FinalizarOpinionExterna.");
            }
            if (!opinionReceptor.Activa)
            {
              throw new InvalidOperationException("La opinión ya se encuentra finalizada.");
            }
            opinionReceptor.Comentarios = finalizarOpinionRequest.Comentarios;
            opinionReceptor.FechaRespuesta = DateTime.Now;
            opinionReceptor.Activa = false;
            opinionReceptor.FinalizadaPor = finalizarOpinionRequest.FinalizadaPor;
            opinionReceptor.SecuenciaFirma = finalizarOpinionRequest.SecuenciaFirma;
            opinionReceptor.CadenaOriginal = finalizarOpinionRequest.CadenaOriginal;
            //Guardar archivos
            foreach (var archivo in finalizarOpinionRequest.Archivos)
            {
              ArchivoOpinion nuevoArchivo = new ArchivoOpinion();
              nuevoArchivo.Ruta = archivo.Ruta;
              nuevoArchivo.Nombre = archivo.Nombre;
              var tipoElemento = archivo.TipoElemento;
              //var elementoEnum = _dbContext.TipoElementoEnums.FirstOrDefault(i => i.Nombre == tipoElemento);  --> Usando BD.
              var elementoEnum = await _elementosRepository.GetByConditionAsync(i => i.Nombre == tipoElemento); // --> Usando repositorio.
              var tipoDocumento = archivo.TipoDocumento;
              //var documentoEnum = _dbContext.TipoDocumentoEnums.FirstOrDefault(i => i.Nombre == tipoDocumento); --> Usando BD.
              var documentoEnum = await _documentosRepository.GetByConditionAsync(i => i.Nombre == tipoDocumento); // --> Usando repositorio.
              nuevoArchivo.IdTipoElemento = elementoEnum.Id;
              nuevoArchivo.IdTipoDocumento = documentoEnum.Id;
              nuevoArchivo.FechaCreacion = archivo.FechaCreacion;
              nuevoArchivo.IdReceptor = opinionReceptor.Id;
              opinionReceptor.ArchivoOpinions.Add(nuevoArchivo);
            }
            // Estado Activo o Inactivo de la opinión.
            // Si se encuentra un receptor con la misma opinión en activo se dejan en activo la opiniones.
            // En caso contrario se puden poner en inactivo la opinión.
            int idOpinion = opinionReceptor.IdOpinion;
            var opiniones = await _opinionReceptorRepository.GetFilteredAsync(i => i.IdOpinion == finalizarOpinionRequest.IdOpinionReceptor && i.Activa == true && i.Id != finalizarOpinionRequest.IdOpinionReceptor);
            //var opiniones = await _opinionReceptorRepository.GetFilteredIncludingAsync(i => i.IdOpinion && i.Activa && i.Id != finalizarOpinionRequest.IdOpinionReceptor);
            //var opiniones = _dbContext.OpinionReceptors.Where(i => i.IdOpinion == idOpinion && i.Activa && i.Id != finalizarOpinionRequest.IdOpinionReceptor).ToList();
            if (!opiniones.Any())
            {
              opinionReceptor.IdOpinionNavigation.Activa = false;
              updateAsunto = true;
            }
            folioAsunto = opinionReceptor.IdOpinionNavigation.FolioAsunto;
            //_dbContext.SaveChanges(); 

            await _opinionReceptorRepository.UpdateAsync(opinionReceptor);

            transaction.Commit();
            if (updateAsunto)
            {
              await CambiarEstatusAsunto(opinionReceptor.Id, folioAsunto);
            }
          }
          else
          {
            throw new InvalidOperationException("No se encontró información con el identificador proporcionado.");
          }
        }
        catch (DbUpdateException)
        {
          transaction.Rollback();
          throw new InvalidOperationException("Ocurrió un error al actualizar la información de la base de datos.");
        }
        catch
        {
          throw;
        }
      }
    }


    /// <summary>
    /// Permite finalizar el proceso de una opinión externa.
    /// </summary>
    /// <param name="opinion"> Información proporcionada por la autoridad que emite su opinión. </param>
    public async Task FinalizarOpinionExterna(OpinionExterna opinion)
    {
      string folioAsunto = string.Empty;
      bool updateAsunto = false;
      using (var transaction = await _opinionReceptorRepository.BeginTransactionAsync())
      {
        try
        {
          var receptores = await _opinionReceptorRepository.GetFilteredIncludingAsync(p => p.IdEnvio == opinion.IdEnvio, i => i.IdOpinionNavigation); // --> Repositorio
          if (receptores != null)
          {
            var receptor = receptores.FirstOrDefault();
            if (receptor != null)
            {
              if (receptor.Activa != true)
              {
                throw new InvalidOperationException("La opinión ya se encuentra finalizada.");
              }
              if (receptor.Interno)
              {
                throw new InvalidOperationException("Para finalizar una opinión interna se debe utilizar el método FinalizarOpinion.");
              }
              receptor.FechaRespuesta = DateTime.Now;
              receptor.Activa = false;

              if (opinion.FirmaElectronica != null)
              {
                receptor.SecuenciaFirma = opinion.FirmaElectronica.Secuencia;
                receptor.CadenaOriginal = opinion.FirmaElectronica.CadenaOriginal;
              }

              var receptoresActivos = await _opinionReceptorRepository.GetFilteredAsync(i => i.IdOpinion == receptor.IdOpinionNavigation.Id && i.Activa && i.Id != receptor.Id);
              if (!receptoresActivos.Any())
              {
                receptor.IdOpinionNavigation.Activa = false;

                updateAsunto = true;

              }

              await _opinionReceptorRepository.UpdateAsync(receptor);

              transaction.Commit();

              folioAsunto = receptor.IdOpinionNavigation.FolioAsunto;
              if (updateAsunto)
              {
                await CambiarEstatusAsunto(receptor.IdEnvio, folioAsunto);
              }
            }
            // var receptoresActivos = _dbContext.OpinionReceptors.Where(o => o.IdOpinion == receptor.IdOpinionNavigation.Id && o.Activa && o.Id != receptor.Id); --> BD
          }
          //}
          else
          {
            throw new InvalidOperationException("No se encontró el registro con el id de envio integrado.");
          }
        }
        catch (DbUpdateException)
        {
          await transaction.RollbackAsync();
          throw new InvalidOperationException("Ocurrió un error al actualizar la información de la base de datos.");
        }
        catch
        {
          await transaction.RollbackAsync();
          throw;
        }
      }
    }

    ///// <summary>
    ///// Obtiene los documentos anexos a una solicitud de opinión.
    ///// </summary>
    ///// <param name="idEnvio"> Identificador del envío configurado en la oficialía de gestión.</param>
    ///// <returns> Arreglo de documentos.</returns>
    //public async Task<ArchivosModelIntern[]> ObtenerArchivosOpinionExterna(string idEnvio)
    //{
    //    try
    //    {
    //        var opinionesReceptor = await _opinionReceptorRepository.GetFilteredAsync(i => i.IdEnvio == idEnvio);
    //        var opinionReceptor = opinionesReceptor.FirstOrDefault();
    //        if (opinionReceptor != null)
    //        {
    //            List<ArchivosModelIntern> archivos = new List<ArchivosModelIntern>();
    //            var opinion = await _opinionRepository.GetByIdIncludingAsync(opinionReceptor.IdOpinion, o => o.ArchivoOpinions);
    //            foreach (var a in opinionReceptor.IdOpinionNavigation.ArchivoOpinions.OrderByDescending(x => x.FechaCreacion))
    //            {
    //                var tipoDocumentoEnum = await _documentosRepository.GetByConditionAsync(i => i.Id == a.IdTipoDocumento);
    //                var tipoElementoEnum = await _elementosRepository.GetByConditionAsync(i => i.Id == a.IdTipoElemento);
    //                archivos.Add(new ArchivosModelIntern
    //                {
    //                    FechaCreacion = a.FechaCreacion,
    //                    Nombre = a.Nombre,
    //                    Ruta = a.Ruta,
    //                    TipoDocumento = tipoDocumentoEnum.Nombre,
    //                    TipoElemento = tipoElementoEnum.Nombre
    //                });
    //            }
    //            return archivos.ToArray();
    //        }
    //        else
    //        {
    //            throw new InvalidOperationException("No se encontró la opinión con el idEnvio proporcionado.");
    //        }
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    /// <summary>
    ///  Buscar los archivos  y el detalle de la opinión padre, relacionada con el idEnvio dado por el usuario.
    /// </summary>
    /// <param name="idEnvio"> idEnvio de la opinión padre.</param>
    /// <returns> Detalle y documentos de la opinión padre relacionada con el idEnvio. </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<DescripcionOpinionExterna> ConsultarOpinionExterna(string idEnvio)
    {
      try
      {
        DescripcionOpinionExterna descripcionOpinion = new DescripcionOpinionExterna();
        var opinionesReceptor = await _opinionReceptorRepository.GetFilteredAsync(i => i.IdEnvio == idEnvio);
        var opinionReceptor = opinionesReceptor.FirstOrDefault();
        if (opinionReceptor != null)
        {
          List<ArchivosModelExt> archivos = new List<ArchivosModelExt>();
          var opinion = await _opinionRepository.GetByIdIncludingAsync(opinionReceptor.IdOpinion, o => o.ArchivoOpinions);
          foreach (var a in opinionReceptor.IdOpinionNavigation.ArchivoOpinions.OrderByDescending(f => f.FechaCreacion))
          {
            var tipoDocumentoEnum = await _documentosRepository.GetByConditionAsync(i => i.Id == a.IdTipoDocumento);
            var tipoElementoEnum = await _elementosRepository.GetByConditionAsync(i => i.Id == a.IdTipoElemento);
            archivos.Add(new ArchivosModelExt
            {
              FechaCreacion = a.FechaCreacion,
              Nombre = a.Nombre,
              Ruta = a.Ruta,
              TipoDocumento = tipoDocumentoEnum.Nombre,
              TipoElemento = tipoElementoEnum.Nombre
            });
          }
          string detalle = opinion.Detalle;
          descripcionOpinion.detalle = detalle;
          descripcionOpinion.documentos = archivos;
          return descripcionOpinion;
        }
        else
        {
          throw new InvalidOperationException("No se encontró el idEnvio solicitado.");
        }
      }
      catch
      {
        throw new InvalidOperationException("Ocurrió un error al consultar la información.");
      }
    }

    public async Task<string[]> SolicitudesPendientesFirma()
    {
      try
      {
        var solicitudes = await _opinionReceptorRepository.GetFilteredIncludingAsync(o => o.EstatusSolicitud != null && o.EstatusSolicitud.Equals("PENDIENTE DE FIRMA"), o => o.IdOpinionNavigation);

        var folios = solicitudes.Select(i => i.IdOpinionNavigation.FolioAsunto);

        return folios.ToArray();
      }
      catch
      {
        throw;
      }
    }

    private async Task<Opinion> GenerarNuevaOpinionAsync(SolicitarOpiniones solicitud)
    {
      var opinionesActivas = await _opinionRepository.GetFilteredAsync(o => o.FolioAsunto == solicitud.FolioAsunto);

      var opinion = new Opinion();

      opinion.FolioAsunto = solicitud.FolioAsunto;
      opinion.FechaSolicitud = DateTime.Now;
      opinion.Detalle = solicitud.Comentarios;
      opinion.Activa = true;

      Opinion? lastOpinion = opinionesActivas.OrderByDescending(o => o.FechaSolicitud).FirstOrDefault();

      opinion.Version = lastOpinion == null ? 1 : lastOpinion.Version + 1;
      opinion.SecuenciaFirma = solicitud.SecuenciaFirma;
      opinion.CadenaOriginal = solicitud.CadenaOriginal;

      await AgregarReceptoresAsync(opinion, solicitud.Receptores, solicitud.Asunto, solicitud.AreaResponsable);

      return opinion;
    }

    private async Task AgregarReceptoresAsync(Opinion opinion, SolicitarOpinionesReceptores[] receptores, Asunto? asunto, int? areaResponsable)
    {
      foreach (var r in receptores)
      {
        OpinionReceptor receptor = new OpinionReceptor();
        receptor.Clave = r.Clave;
        receptor.Nombre = r.Nombre;
        receptor.Obligatoria = r.EsObligatoria;
        receptor.Interno = r.EsInterna;
        receptor.Activa = true;
        receptor.IdOpinion = opinion.Id;
        receptor.Firmante = r.firmante;
        receptor.EstatusSolicitud = r.estatusSolicitud;

        if (!r.EsInterna)
        {
          string jsonCompleto = await ObtenerDisenoEnvioOpinion(asunto, areaResponsable.Value);

          string folioEnvio = await ObtenerIdEnvio(jsonCompleto, r.EntidadExterna);

          receptor.IdEnvio = folioEnvio;
        }

        opinion.OpinionReceptors.Add(receptor);
      }
    }

    private async Task AgregarArchivosAsync(Opinion opinion, ArchivosModel[] archivos)
    {
      foreach (var a in archivos)
      {
        var archivo = new ArchivoOpinion
        {
          Ruta = a.Ruta,
          FechaCreacion = a.FechaCreacion,
          IdTipoElemento = (await _elementosRepository.GetByConditionAsync(t => t.Nombre == a.TipoElemento)).Id,
          IdTipoDocumento = (await _documentosRepository.GetByConditionAsync(t => t.Nombre == a.TipoDocumento)).Id,
          IdOpinion = opinion.Id,
          Nombre = a.Nombre
        };

        //// Se agrega un nuevo elemento de tipo Archivo opinión que está vinculado a la nueva opinión que se está generando.
        opinion.ArchivoOpinions.Add(archivo);
      }
    }

    private async Task<string> ObtenerDisenoEnvioOpinion(Asunto? asunto, int areaResponsable)
    {
      try
      {
        string apimFqdn = await _keyVaultService.GetSecretValueAsync("CNBV--Apim");
        string subscriptionKey = await _keyVaultService.GetSecretValueAsync("CNBV--PortalGestion--Servicios--Ocp-Apim-Subscription-Key");

        var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

        string url = string.Format("{0}/gestion-diseno-envios/v1/DisenoEnvios/B025E4EC-42A7-45EF-A879-22D6A6F91F52", apimFqdn);

        // Realizando la llamada de manera sincrónica
        var response = await httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        if (response.IsSuccessStatusCode)
        {
          string responseBody = response.Content.ReadAsStringAsync().Result;

          // Parsear el JSON a un objeto JsonNode
          JsonNode jsonNode = JsonNode.Parse(responseBody);

          // Añadir las nuevas propiedades
          jsonNode["idAreaResponsable"] = areaResponsable;
          jsonNode["folioCrearAsunto"] = asunto.folio;
          jsonNode["idAsunto"] = asunto.id;

          // Convertir el objeto modificado a una cadena JSON
          string jsonModificado = jsonNode.ToJsonString();

          return jsonModificado;
        }
        else
        {
          throw new InvalidOperationException("Ocurrió un error al intentar obtener el diseño del envío de la opinión.");
        }
      }
      catch (HttpRequestException)
      {
        throw new InvalidOperationException("No se obtuvo una respuesta satisfactoria al momento de crear la opinión externa.");
      }
      catch
      {
        throw new InvalidOperationException("Ocurrió un error al momento de crear la opinión externa.");
      }
    }

    private async Task<string> ObtenerIdEnvio(string jsonCompleto, Entidad entidad)
    {
      string apimFqdn = await _keyVaultService.GetSecretValueAsync("CNBV--Apim");
      string subscriptionKey = await _keyVaultService.GetSecretValueAsync("CNBV--PortalGestion--Servicios--Ocp-Apim-Subscription-Key");

      var httpClient = new HttpClient();

      httpClient.BaseAddress = new Uri(apimFqdn + "/");

      httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

      HttpContent content = new StringContent(jsonCompleto, Encoding.UTF8, "application/json");

      HttpResponseMessage response = await httpClient.PostAsync(string.Format("gestor-de-envios/v1/GestorEnvios/{0}/{1}", entidad.Id, entidad.Tipo), content);

      if (response.IsSuccessStatusCode)
      {
        string responseBody2 = await response.Content.ReadAsStringAsync();

        JsonNode jsonNode = JsonNode.Parse(responseBody2);

        string folioEnvio = jsonNode["respuesta"]["folioEnvio"].ToString();

        return folioEnvio;
      }
      else
      {
        throw new InvalidOperationException("Ocurrió un error al intentar generar el envío de la opinión.");
      }
    }

    private async Task CambiarEstatusAsunto<T>(T idReceptor, string folioAsunto)
    {
      try
      {
        var httpClient = new HttpClient();

        string asuntosIp = await _keyVaultService.GetSecretValueAsync("CNBV--MS--Asuntos--IP");

        httpClient.BaseAddress = new Uri(asuntosIp);

        GestorAsuntoRequest requestBody = new GestorAsuntoRequest();
        requestBody.FolioAsunto = folioAsunto;
        requestBody.IdEstatusAsunto = 4;
        requestBody.Accion = "SOLICITUD_FINALIZADA";

        string json = JsonSerializer.Serialize(requestBody);

        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PostAsync("actualizarasunto/estatus", content);

        if (!response.IsSuccessStatusCode)
        {
          RollbackOpinionGeneric(idReceptor);
          throw new InvalidOperationException("No fue posible cambiar el estatus del asunto. El estatus de la opinión no fue afectado.");
        }
      }
      catch (InvalidOperationException)
      {
        throw;
      }
      catch
      {
        RollbackOpinionGeneric(idReceptor);
        throw new InvalidOperationException("Ocurrió un error al intentar cambiar el estatus del asunto. El estatus de la opinión no fue afectado.");
      }
    }

    private void RollbackOpinionGeneric<T>(T idReceptor)
    {
      if (typeof(T) == typeof(string))
      {
        RollbackOpinion(idReceptor, r => r.IdEnvio == idReceptor.ToString());
      }
      else if (typeof(T) == typeof(int))
      {
        RollbackOpinion(idReceptor, r => r.Id == int.Parse(idReceptor.ToString()));
      }
    }

    /// <summary>
    /// Permite hacer un rollback al proceso de cierre de una opinión de un receptor y la opinión origen.
    /// </summary>
    /// <typeparam name="T">Valor dinámico para el parámetro de búsqueda de la opinión del receptor.</typeparam>
    /// <param name="idEnvio">Identificador del envío generado para la opinión externa.</param>
    /// <param name="filter"></param>
    /// <exception cref="InvalidOperationException">Excepción generada si existe un error controlado dentro del metodo.</exception>
    private async void RollbackOpinion<T>(T idReceptor, Expression<Func<OpinionReceptor, bool>> filter)
    {
      using (
                var transaction = await _opinionRepository.BeginTransactionAsync())
      {
        try
        {
          var receptor = await _opinionReceptorRepository.GetByConditionIncludingAsync(filter, o => o.ArchivoOpinions);

          if (!receptor.Activa)
          {
            // Revertir las propiedades de este objeto.
            receptor.Activa = true;
            receptor.FechaRespuesta = null;
            receptor.SecuenciaFirma = null;
            receptor.CadenaOriginal = null;

            if (receptor.Interno)
            {
              receptor.Comentarios = null;
              receptor.FinalizadaPor = null;

              receptor.ArchivoOpinions.Clear();
            }

            // Revertir el estatus de la opinión a la que pertenece.
            var opinion = await _opinionRepository.GetByIdIncludingAsync(receptor.IdOpinion, o => o.OpinionReceptors);

            if (!opinion.Activa)
            {
              opinion.Activa = true;
            }
            transaction.Commit();
          }
        }
        catch
        {
          transaction.Rollback();
          throw new InvalidOperationException("Ocurrió un error al hacer rollback de la operación.");
        }
      }
    }

    /// <summary>
    /// Actualiza la opinion.
    /// </summary>
    /// <param name="ActOpinion">Opinion a actualizar</param>
    /// <returns></returns>
    public async Task<string> ActualizarOpinion(ActualizarOpinion ActOpinion)
    {
      using (var transaction = await _opinionRepository.BeginTransactionAsync())
      {
        try
        {
          // Obtener la opinión existente
          var opinion = await _opinionRepository.GetByIdIncludingAsync(ActOpinion.Id, o => o.OpinionReceptors, o => o.ArchivoOpinions);
          if (opinion == null)
          {
            return "Opinión no encontrada.";
          }

          // Actualizar campos de la opinión
          opinion.SecuenciaFirma = ActOpinion.SecuenciaFirma;
          opinion.CadenaOriginal = ActOpinion.CadenaOriginal;
          opinion.Detalle = ActOpinion.Comentarios;

          // Actualizar archivos y receptores de opinión
          foreach (var recepOpinion in ActOpinion.Receptor)
          {
            var recep = await _opinionReceptorRepository.GetByIdIncludingAsync(recepOpinion.id, o => o.ArchivoOpinions) ?? new OpinionReceptor();

            recep.Firmante = recepOpinion.Firmante;
            recep.EstatusSolicitud = recepOpinion.EstatusSolicitud;
            recep.ComentarioFirmante = recepOpinion.ComentarioFirmante;

            foreach (var archivoOpinion in recepOpinion.Archivos)
            {
              var archivo = opinion.ArchivoOpinions.FirstOrDefault(a => a.Id == archivoOpinion.id) ?? new ArchivoOpinion();

              archivo.Ruta = archivoOpinion.Ruta;
              archivo.Nombre = archivoOpinion.Nombre;
              archivo.FechaCreacion = archivoOpinion.FechaCreacion.Value;
              archivo.IdTipoElemento = (await _elementosRepository.GetByConditionAsync(e => e.Nombre == archivoOpinion.TipoElemento)).Id;
              archivo.IdTipoDocumento = (await _documentosRepository.GetByConditionAsync(d => d.Nombre == archivoOpinion.TipoDocumento)).Id;
              archivo.Eliminado = archivoOpinion.eliminado;

              // if (archivoOpinion.id == 0)
              // {
              //     opinion.ArchivoOpinions.Add(archivo);
              // }
              recep.ArchivoOpinions.Add(archivo);
            }
            await _opinionReceptorRepository.UpdateAsync(recep);

            // if (recepOpinion.id == 0)
            // {
            //     opinion.OpinionReceptors.Add(recep);
            // }
          }

          foreach (var archivoOpinion in ActOpinion.Archivos)
          {
            var archivo = opinion.ArchivoOpinions.FirstOrDefault(a => a.Id == archivoOpinion.id) ?? new ArchivoOpinion();

            archivo.Ruta = archivoOpinion.Ruta;
            archivo.Nombre = archivoOpinion.Nombre;
            archivo.FechaCreacion = archivoOpinion.FechaCreacion.Value;
            archivo.IdTipoElemento = (await _elementosRepository.GetByConditionAsync(e => e.Nombre == archivoOpinion.TipoElemento)).Id;
            archivo.IdTipoDocumento = (await _documentosRepository.GetByConditionAsync(d => d.Nombre == archivoOpinion.TipoDocumento)).Id;
            archivo.Eliminado = archivoOpinion.eliminado;

            if (archivoOpinion.id == 0)
            {
              opinion.ArchivoOpinions.Add(archivo);
            }
          }

          // Actualizar la opinión en la base de datos
          await _opinionRepository.UpdateAsync(opinion);

          await transaction.CommitAsync();

          return "Opinión actualizada correctamente.";
        }
        catch (Exception ex)
        {
          await transaction.RollbackAsync();
          return $"Error al actualizar la opinión: {ex.Message}";
        }
      }
    }

  }

}

