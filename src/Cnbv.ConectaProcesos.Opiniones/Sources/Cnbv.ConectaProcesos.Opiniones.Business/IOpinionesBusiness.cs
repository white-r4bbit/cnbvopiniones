using Cnbv.ConectaProcesos.Opiniones.Data.DatabaseConectaProcesos;
using Cnbv.ConectaProcesos.Opiniones.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cnbv.ConectaProcesos.Opiniones.Business
{
    public interface IOpinionesBusiness
    {
        /// <summary>
        /// Operación para guardar la información de una opinión.
        /// </summary>
        /// <param name="solicitud"> DTO que guarda la información necesaria para registrar una opinión. </param>
        /// <returns> Code response y string (identificador o mensaje con el mensaje de error). </returns>
        Task<int> SolicitarOpinionesAsync(SolicitarOpiniones solicitud);


        /// <summary>
        /// Operación para mandar a traer todas las opiniones asociadas a un folio.
        /// </summary>
        /// <param name="folio"> Folio asignado a la opinión. </param>
        /// <returns> Lista de las opiniones asociadas al folio que dió el usuario. </returns>
        Task<List<ObtenerOpinionesModel>> ObtenerOpinionesAsync(string folio);

        /// <summary>
        /// Operación para traer más información sobre una opinión en especial.
        /// </summary>
        /// <param name="idOpinionReceptor"> id de la opinion de cada receptor (id asignado en la tabla OpinionReceptor) </param>
        /// <returns> Detalle de la opinión asignada a un receptor. </returns>
        Task<ObtenerDetalleOpinion> ObtenerDetalleOpinion(int idOpinionReceptor);

        /// <summary>
        /// Función para subir los archivos del usuario a la base de datos.
        /// </summary>
        /// <param name="archivosRequest"> Información para almacenar un archivo. </param>
        /// <returns></returns>
        Task AgregarArchivos(ArchivosRequest archivosRequest);

        /// <summary>
        /// Operación para guardar la respuesta y archivos que se envian como respuesta por parte del receptor
        /// </summary>
        /// <param name="finalizarOpinionRequest"></param>
        /// <returns></returns>
        Task FinalizarOpinion(FinalizarOpinion finalizarOpinionRequest);

        /// <summary>
        /// Permite finalizar el proceso de una opinión externa.
        /// </summary>
        /// <param name="opinion"></param>
        Task FinalizarOpinionExterna(OpinionExterna opinion);

        /// <summary>
        /// Obtiene los documentos anexos a una solicitud de opinión.
        /// </summary>
        /// <param name="idEnvio"></param>
        /// <returns></returns>
       // Task<ArchivosModelExt[]> ObtenerArchivosOpinionExterna(string idEnvio);

        /// Función para buscar los archivos y el detalle de la opinión padre, relacionada con el idEnvio dado por el usuario.
        Task<DescripcionOpinionExterna> ConsultarOpinionExterna(string idEnvio);
    }
}
