using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
  public class FinalizarOpinion
  {
    public int IdOpinionReceptor { get; set; }

    public string? FinalizadaPor { get; set; }

    public string Comentarios { get; set; }

    public ArchivosModel[] Archivos { get; set; }

    public int? SecuenciaFirma { get; set; }

    public string? CadenaOriginal { get; set; }

  }
}
