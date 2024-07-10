using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
    public class ObtenerOpinionesElementosOpiniones
    {
        public string Id { get; set; }

        public string FolioAsunto { get; set; }

        public DateTime FechaDeSolicitud { get; set; }

        public bool Activa { get; set; }

        public int Version { get; set; }

        public ObtenerOpinionesReceptores[] Receptores { get; set; } 
    }
}
