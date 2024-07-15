using Cnbv.ConectaProcesos.Opiniones.Data.DatabaseConectaProcesos;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Cnbv.ConectaProcesos.Opiniones.Business;
using Cnbv.ConectaProcesos.Opiniones.Entities;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Cnbv.ConectaProcesos.Opiniones.Common;
using Newtonsoft.Json;
using Microsoft.ApplicationInsights;
using System.Text.Json.Nodes;

namespace Cnbv.ConectaProcesos.Opiniones.Api.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [Tags("Solicitud de Opiniones")]
  [ApiController]
  [Route("api/[controller]")]
  public class OpinionesController : ControllerBase
  {
    private readonly IOpinionesBusiness _businessLayer;
    private readonly AppInsightsService _insightsService;
    private readonly IHttpContextAccessor _contextAccessor;

    private readonly KeyVaultService _keyVaultService;

    /// <summary>
    /// Inyecci�n de datos.
    /// </summary>
    /// <param name="businessLayer"></param>
    /// <param name="insightsService"></param>
    /// <param name="contextAccessor"></param>
    public OpinionesController(IOpinionesBusiness businessLayer, AppInsightsService insightsService, IHttpContextAccessor contextAccessor, KeyVaultService keyVaultService)
    {
      _businessLayer = businessLayer;
      _insightsService = insightsService;
      _contextAccessor = contextAccessor;
      _keyVaultService = keyVaultService;
    }


    /// <summary>
    /// Permite generar un proceso de solicitud de opini�n.
    /// </summary>
    /// <remarks>
    /// Permite generar un proceso de solicitud de opini�n de manera interna (�reas de la CNBV) y externa (Autoridades) sobre un folio asunto.
    /// </remarks>
    /// <param name="opinionesModel">Informaci�n del solicitante y los receptores.</param>
    /// <returns>"true" si la operaci�n se ejecut� correctamente, "false" en caso contrario.</returns>
    [HttpPost]
    [Route("solicitaropiniones")]
    [ProducesResponseType(typeof(OpinionCreateResponse), 201)]
    [ProducesResponseType(typeof(string), 409)]
    [ProducesResponseType(typeof(string), 500)]
    [SwaggerOperation(OperationId = "SolicitarOpiniones")]
    public async Task<ActionResult> SolicitarOpinionesAsync([FromBody] SolicitarOpiniones opinionesModel)
    {
      try
      {
        string jsonData = JsonConvert.SerializeObject(opinionesModel);

        _insightsService.TrackEvent(nameof(OpinionesController), new Dictionary<string, string> { { "Se ejecuta metodo => SolicitarOpiniones", $"{jsonData}" } }, GetClientIPAddress());

        var response = await _businessLayer.SolicitarOpinionesAsync(opinionesModel);

        return StatusCode(StatusCodes.Status201Created, response);
      }
      catch (InvalidOperationException ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status409Conflict, ex.Message);
      }
      catch (DbUpdateException ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status500InternalServerError, string.Format(ex.Message));
      }
      catch (Exception ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status500InternalServerError, string.Format("Ocurri� un error general: {0}", ex.Message));
      }
      finally
      {
      }
    }


    /// <summary>
    /// Permite traer la lista de opiniones relacionadas a un folio asunto.
    /// </summary>
    /// <remarks>
    /// Permite traer la lista de opiniones solicitadas a diferentes receptores ya sean internos o externos a la CNBV.
    /// </remarks>
    /// <param name="folio">Folio asignado a la opini�n.</param>
    /// <returns> Lista de las opiniones asociadas al folio que di� el usuario. </returns>
    [HttpGet]
    [Route("obteneropiniones")]
    [ProducesResponseType(typeof(List<ObtenerOpinionesModel>), 200)]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(string), 500)]
    [SwaggerOperation(OperationId = "ObtenerOpiniones")]
    public async Task<ActionResult<List<ObtenerOpinionesModel>>> ObtenerOpinionesAsync(string folio)
    {
      try
      {
        string jsonData = JsonConvert.SerializeObject(folio);

        _insightsService.TrackEvent(nameof(OpinionesController), new Dictionary<string, string> { { "Se ejecuta metodo => ObtenerOpiniones", $"{jsonData}" } }, GetClientIPAddress());

        List<ObtenerOpinionesModel> opinionesResponse = new List<ObtenerOpinionesModel>();
        opinionesResponse = await _businessLayer.ObtenerOpinionesAsync(folio);

        return Ok(new
        {
          opiniones = opinionesResponse
        });
      }
      catch (InvalidOperationException ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status404NotFound, ex.Message);
      }
      catch (Exception ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status500InternalServerError, string.Format("Ocurri� un error general: {0}", ex.Message));
      }
    }


    /// <summary>
    /// Permite traer el detalle de la opini�n y de los receptores con base en el folio asunto.
    /// </summary>
    /// <remarks>
    /// Permite traer los comentarios y archivos de la opinion y del receptor con base en el id del mismo.
    /// </remarks>
    /// <param name="idOpinionReceptor"> N�mero de ID que le correspenda a la opi�n que se le solicit� a un receptor.</param>
    /// <returns>"true" si la operaci�n se ejecut� correctamente, "false" en caso contrario.</returns>
    [HttpGet]
    [Route("obtenerdetalleopinion")]
    [ProducesResponseType(typeof(ObtenerDetalleOpinion), 200)]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(string), 500)]
    [SwaggerOperation(OperationId = "ObtenerDetalleOpinion")]
    public async Task<ActionResult> ObtenerDetalleOpinion(int idOpinionReceptor)
    {
      try
      {
        string jsonData = JsonConvert.SerializeObject(idOpinionReceptor);

        _insightsService.TrackEvent(nameof(OpinionesController), new Dictionary<string, string> { { "Se ejecuta metodo => ObtenerDetalleOpinion", $"{jsonData}" } }, GetClientIPAddress());

        var detalleOpinion = await _businessLayer.ObtenerDetalleOpinion(idOpinionReceptor);

        return Ok(new
        {
          opinion = detalleOpinion
        });
      }
      catch (InvalidOperationException ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status404NotFound, ex.Message);
      }
      catch (Exception ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status500InternalServerError, string.Format("Ocurri� un error general: {0}", ex.Message));
      }
    }

    /// <summary>
    /// Esta Operaci�n permite cargar una lista de archivos.
    /// </summary>
    /// <param name="archivosRequest">Json con la informaci�n de los archivos.</param>
    /// <returns></returns>
    [HttpPatch]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(string), 409)]
    [ProducesResponseType(typeof(string), 500)]
    [Route("agregarArchivos")]
    [SwaggerOperation(OperationId = "AgregarArchivos")]
    public async Task<ActionResult> AgregarArchivos([FromBody] ArchivosRequest archivosRequest)
    {
      try
      {
        string jsonData = JsonConvert.SerializeObject(archivosRequest);

        _insightsService.TrackEvent(nameof(OpinionesController), new Dictionary<string, string> { { "Se ejecuta metodo => CargarArchivosAsync", $"{jsonData}" } }, GetClientIPAddress());

        await _businessLayer.AgregarArchivos(archivosRequest);

        return StatusCode(StatusCodes.Status200OK, string.Format("Los archivos fueron almacenados correctamente."));

      }
      catch (InvalidOperationException ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status404NotFound, ex.Message);
      }
      catch (DbUpdateException ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status409Conflict, "Ocurri� un error al intentar guardar la informaci�n.");
      }
      catch (Exception ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status500InternalServerError, string.Format("Ocurri� un error general: {0}", ex.Message));
      }
    }

    /// <summary>
    /// Permite guardar la informaci�n de respuesta que da el receptor y cierra la opini�n.
    /// </summary>
    /// <remarks>
    /// Permite guardar la informaci�n que haya respondido el receptor de la opini�n, los archivos que agreg� a su respuesta, sus comentarios y se cierra la opini�n.
    /// </remarks>
    /// <param name="finalizarOpinionRequest"></param>
    /// <returns></returns>
    [HttpPatch]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(string), 409)]
    [ProducesResponseType(typeof(string), 500)]
    [Route("finalizaropinion")]
    [SwaggerOperation(OperationId = "FinalizarOpinion")]
    public async Task<ActionResult> FinalizarOpinion([FromBody] FinalizarOpinion finalizarOpinionRequest)
    {
      try
      {
        string jsonData = JsonConvert.SerializeObject(finalizarOpinionRequest);

        _insightsService.TrackEvent(nameof(OpinionesController), new Dictionary<string, string> { { "Se ejecuta metodo => FinalizarOpinion", $"{jsonData}" } }, GetClientIPAddress());

        await _businessLayer.FinalizarOpinion(finalizarOpinionRequest);

        return StatusCode(StatusCodes.Status200OK, "La opini�n fue finalizada correctamente.");
      }
      catch (InvalidOperationException ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status404NotFound, ex.Message);
      }
      catch (DbUpdateException ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status409Conflict, "Ocurri� un error al intentar guardar la informaci�n.");
      }
      catch (Exception ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status500InternalServerError, string.Format("Ocurri� un error general: {0}", ex.Message));
      }
    }

    /// <summary>
    /// Permite finalizar el proceso de una opini�n externa.
    /// </summary>
    /// <param name="opinionExterna">Informaci�n proporcionada por la autoridad que emite su opini�n.</param>
    /// <returns>Bandera que indica que el proceso se ejecut� correctamente as� como un mensaje complementario.</returns>
    [HttpPatch]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(string), 409)]
    [ProducesResponseType(typeof(string), 500)]
    [Route("finalizaropinionexterna")]
    [SwaggerOperation(OperationId = "FinalizarOpinionExterna")]
    public async Task<ActionResult> FinalizarOpinionExterna([FromBody] OpinionExterna opinionExterna)
    {
      try
      {
        string jsonData = JsonConvert.SerializeObject(opinionExterna);

        _insightsService.TrackEvent(nameof(OpinionesController), new Dictionary<string, string> { { "Se ejecuta metodo => FinalizarOpinionExterna", $"{jsonData}" } }, GetClientIPAddress());

        await _businessLayer.FinalizarOpinionExterna(opinionExterna);

        return StatusCode(StatusCodes.Status200OK, "La opini�n fue finalizada correctamente.");
      }
      catch (InvalidOperationException ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status404NotFound, ex.Message);
      }
      catch (DbUpdateException ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status409Conflict, string.Format("Ocurri� un error al intentar guardar la informaci�n: {0}.", ex.Message));
      }
      catch (Exception ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status500InternalServerError, string.Format("Ocurri� un error general: {0}", ex.Message));
      }
    }

    /// <summary>
    /// Obtiene los documentos y el detalle anexos a una solicitud de opini�n.
    /// </summary>
    /// <param name="idEnvio">Identificador del env�o configurado en la oficial�a de gesti�n.</param>
    /// <returns>Arreglo de documentos y detlle de la opini�n externa. </returns>
    [HttpGet]
    [Route("consultaropinionexterna")]
    [ProducesResponseType(typeof(DescripcionOpinionExterna), 200)]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(string), 500)]
    [SwaggerOperation(OperationId = "ConsultarOpinionExterna")]
    public async Task<ActionResult> ConsultarOpinionExterna(string idEnvio)
    {
      try
      {
        string jsonData = JsonConvert.SerializeObject(idEnvio);

        _insightsService.TrackEvent(nameof(OpinionesController), new Dictionary<string, string> { { "Se ejecuta metodo => ConsultarOpinion", $"{jsonData}" } }, GetClientIPAddress());

        DescripcionOpinionExterna detalleOpinion = new DescripcionOpinionExterna();
        detalleOpinion = await _businessLayer.ConsultarOpinionExterna(idEnvio);

        return Ok(new
        {
          detalle = detalleOpinion.detalle,
          archivos = detalleOpinion.documentos
        });
      }
      catch (InvalidOperationException ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status404NotFound, ex.Message);
      }
      catch (Exception ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status500InternalServerError, string.Format("Ocurri� un error general: {0}", ex.Message));
      }
    }

    /// <summary>
    /// Actualiza la opinion.
    /// </summary>
    /// <param name="ActOpinion"> Opinion a actualizar </param>
    /// <returns></returns>
    [HttpPatch]
    [Route("ActualizarOpinion")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(string), 500)]
    [SwaggerOperation(OperationId = "ActualizarOpinion")]
    public async Task<ActionResult> ActualizarOpinion(ActualizarOpinion actOpinion)
    {
      try
      {
        string jsonData = JsonConvert.SerializeObject(actOpinion);

        _insightsService.TrackEvent(nameof(OpinionesController), new Dictionary<string, string> { { "Se ejecuta metodo => BorradoLogicoArchivoOpinion", $"{jsonData}" } }, GetClientIPAddress());

        var resultado = await _businessLayer.ActualizarOpinion(actOpinion);

        if (resultado == "Archivo de opinión no encontrado.")
        {
          return StatusCode(StatusCodes.Status404NotFound, resultado);
        }

        return StatusCode(StatusCodes.Status200OK, resultado);
      }
      catch (Exception ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status500InternalServerError, string.Format("Ocurrió un error general: {0}", ex.Message));
      }
    }

    [HttpGet]
    [Route("ObtenerAsuntosPendienteFirma")]
    [ProducesResponseType(typeof(string[]), 200)]
    [ProducesResponseType(typeof(string), 500)]
    [SwaggerOperation(OperationId = "ObtenerAsuntosPendienteFirma")]
    public async Task<ActionResult> ObtenerAsuntosPendienteFirma(string firmante)
    {
      try
      {
        var folios = await _businessLayer.MetodoPrueba(firmante);
        return StatusCode(StatusCodes.Status200OK, folios);
      }
      catch (Exception ex)
      {
        _insightsService.TrackException(ex, GetClientIPAddress());
        return StatusCode(StatusCodes.Status500InternalServerError, string.Format("Ocurri� un error general: {0}", ex.Message));
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private string GetClientIPAddress()
    {
      string ipAddress = _contextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();

      return ipAddress;
    }
  }
}