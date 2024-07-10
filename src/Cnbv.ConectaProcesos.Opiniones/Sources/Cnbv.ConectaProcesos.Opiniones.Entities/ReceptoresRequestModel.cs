using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
    public class ReceptoresRequestModel
    {
        public string Clave { get; set; }

        public bool EsInterna { get; set; }

        public bool EsObligatoria { get; set; }
    }
}
