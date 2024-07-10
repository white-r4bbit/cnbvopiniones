using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
    public class DescripcionOpinionExterna
    {
        public string detalle { get; set; }

        public List<ArchivosModelExt> documentos { get; set; }
    }
}
