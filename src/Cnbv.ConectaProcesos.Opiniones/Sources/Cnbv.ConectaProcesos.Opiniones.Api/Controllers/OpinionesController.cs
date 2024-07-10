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
        /// Inyección de datos.
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
        /// Permite generar un proceso de solicitud de opinión.
        /// </summary>
        /// <remarks>
        /// Permite generar un proceso de solicitud de opinión de manera interna (áreas de la CNBV) y externa (Autoridades) sobre un folio asunto.
        /// </remarks>
        /// <param name="opinionesModel">Información del solicitante y los receptores.</param>
        /// <returns>"true" si la operación se ejecutó correctamente, "false" en caso contrario.</returns>
        [HttpPost]
        [Route("solicitaropiniones")]
        [ProducesResponseType(typeof(int), 201)]
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
                return StatusCode(StatusCodes.Status500InternalServerError, string.Format("Ocurrió un error general: {0}", ex.Message));
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
        /// <param name="folio">Folio asignado a la opinión.</param>
        /// <returns> Lista de las opiniones asociadas al folio que dió el usuario. </returns>
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
                return StatusCode(StatusCodes.Status500InternalServerError, string.Format("Ocurrió un error general: {0}", ex.Message));
            }
        }


        /// <summary>
        /// Permite traer el detalle de la opinión y de los receptores con base en el folio asunto.
        /// </summary>
        /// <remarks>
        /// Permite traer los comentarios y archivos de la opinion y del receptor con base en el id del mismo.
        /// </remarks>
        /// <param name="idOpinionReceptor"> Número de ID que le correspenda a la opión que se le solicitó a un receptor.</param>
        /// <returns>"true" si la operación se ejecutó correctamente, "false" en caso contrario.</returns>
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
                return StatusCode(StatusCodes.Status500InternalServerError, string.Format("Ocurrió un error general: {0}", ex.Message));
            }
        }


        /// <summary>
        /// Esta Operación permite cargar una lista de archivos.
        /// </summary>
        /// <param name="archivosRequest">Json con la información de los archivos.</param>
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
                return StatusCode(StatusCodes.Status409Conflict, "Ocurrió un error al intentar guardar la información.");
            }
            catch (Exception ex)
            {
                _insightsService.TrackException(ex, GetClientIPAddress());
                return StatusCode(StatusCodes.Status500InternalServerError, string.Format("Ocurrió un error general: {0}", ex.Message));
            }
        }



        /// <summary>
        /// Permite guardar la información de respuesta que da el receptor y cierra la opinión.
        /// </summary>
        /// <remarks>
        /// Permite guardar la información que haya respondido el receptor de la opinión, los archivos que agregó a su respuesta, sus comentarios y se cierra la opinión.
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

                return StatusCode(StatusCodes.Status200OK, "La opinión fue finalizada correctamente.");
            }
            catch (InvalidOperationException ex)
            {
                _insightsService.TrackException(ex, GetClientIPAddress());
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (DbUpdateException ex)
            {
                _insightsService.TrackException(ex, GetClientIPAddress());
                return StatusCode(StatusCodes.Status409Conflict, "Ocurrió un error al intentar guardar la información.");
            }
            catch (Exception ex)
            {
                _insightsService.TrackException(ex, GetClientIPAddress());
                return StatusCode(StatusCodes.Status500InternalServerError, string.Format("Ocurrió un error general: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Permite finalizar el proceso de una opinión externa.
        /// </summary>
        /// <param name="opinionExterna">Información proporcionada por la autoridad que emite su opinión.</param>
        /// <returns>Bandera que indica que el proceso se ejecutó correctamente así como un mensaje complementario.</returns>
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

                return StatusCode(StatusCodes.Status200OK, "La opinión fue finalizada correctamente.");
            }
            catch (InvalidOperationException ex)
            {
                _insightsService.TrackException(ex, GetClientIPAddress());
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (DbUpdateException ex)
            {
                _insightsService.TrackException(ex, GetClientIPAddress());
                return StatusCode(StatusCodes.Status409Conflict, string.Format("Ocurrió un error al intentar guardar la información: {0}.", ex.Message));
            }
            catch (Exception ex)
            {
                _insightsService.TrackException(ex, GetClientIPAddress());
                return StatusCode(StatusCodes.Status500InternalServerError, string.Format("Ocurrió un error general: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Obtiene los documentos y el detalle anexos a una solicitud de opinión.
        /// </summary>
        /// <param name="idEnvio">Identificador del envío configurado en la oficialía de gestión.</param>
        /// <returns>Arreglo de documentos y detlle de la opinión externa. </returns>
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
                 detalleOpinion   = await _businessLayer.ConsultarOpinionExterna(idEnvio);

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
                return StatusCode(StatusCodes.Status500InternalServerError, string.Format("Ocurrió un error general: {0}", ex.Message));
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