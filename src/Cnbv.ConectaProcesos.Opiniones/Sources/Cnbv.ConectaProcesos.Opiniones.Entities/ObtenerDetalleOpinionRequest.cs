using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
    public class ObtenerDetalleOpinionRequest
    {
        public int IdentidicadorOpinion { get; set; }

        public int IdentificadorOpinionReceptor { get; set; }
    }
}
