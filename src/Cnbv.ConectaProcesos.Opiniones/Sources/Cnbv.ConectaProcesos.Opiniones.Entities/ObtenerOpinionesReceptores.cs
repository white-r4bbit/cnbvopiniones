using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
  public class ObtenerOpinionesReceptores
  {
    public int Id { get; set; }

    public string Clave { get; set; }

    public string Nombre { get; set; }

    public bool EsInterna { get; set; }

    public DateTime? FechaRespuesta { get; set; }

    public bool Obligatoria { get; set; }

    public bool EnProceso { get; set; }

    public string? IdEnvio { get; set; }

    public string Firmante { get; set; }

    public string EstatusSolicitud { get; set; }

    public string ComentarioFirmante { get; set; }
  }
}
