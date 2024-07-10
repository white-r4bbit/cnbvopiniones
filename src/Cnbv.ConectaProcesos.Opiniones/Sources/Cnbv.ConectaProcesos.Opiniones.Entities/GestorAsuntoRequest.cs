using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
  public class GestorAsuntoRequest
  {
    public string FolioAsunto { get; set; }

    public int IdEstatusAsunto { get; set; }

    public string Accion { get; set; }
  }
}
