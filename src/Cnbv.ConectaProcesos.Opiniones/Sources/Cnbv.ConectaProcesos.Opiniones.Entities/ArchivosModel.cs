using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
    public class ArchivosModel
    {
        public string Ruta { get; set; }

        public DateTime FechaCreacion { get; set; }

        public string Nombre { get; set; }

        public string TipoElemento { get; set; }

        public string TipoDocumento { get; set; }
    }
}
