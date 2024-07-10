using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
    public class OpinionDetalle
    {
        public string DetalleSolicitante { get; set; }

        public ArchivosModel[] Archivos { get; set; }

        public ObtenerDetalleOpinionReceptor Receptor { get; set; }

    }
}
