using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
  public class OpinionCreateResponse
  {
    public int Id { get; set; }

    public int[] Receptores { get; set; }
  }
}
