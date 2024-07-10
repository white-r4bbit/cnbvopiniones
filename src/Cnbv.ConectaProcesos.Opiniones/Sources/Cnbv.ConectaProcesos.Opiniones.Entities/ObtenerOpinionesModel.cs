using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
    public class ObtenerOpinionesModel
    {
        public int Identificador { get; set; }

        public string FolioAsunto { get; set; }

        public DateTime FechaSolicitud { get; set; }

        public bool EnProceso { get; set; }

        public int Version { get; set; }

        public ObtenerOpinionesReceptores[] Receptores { get; set; } 

    }
}
