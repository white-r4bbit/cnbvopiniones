using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
  public class ObtenerDetalleOpinionReceptor
  {
    public int Id { get; set; }

    public string Clave { get; set; }

    public string Nombre { get; set; }

    public string Comentarios { get; set; }

    public string? FinalizadaPor { get; set; }

    public DateTime? FechaRespuesta { get; set; }

    public List<ArchivosModelResponse> Archivos { get; set; }

    public string? IdEnvio { get; set; }

    public string Firmante { get; set; }

    public string EstatusSolicitud { get; set; }

    public string ComentarioFirmante { get; set; }
  }
}
