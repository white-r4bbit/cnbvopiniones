using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
  public class SolicitarOpiniones
  {
    public string FolioAsunto { get; set; }

    public SolicitarOpinionesReceptores[] Receptores { get; set; }

    //    public ArchivosModel[] Archivos { get; set; }

    public string Comentarios { get; set; }

    public int SecuenciaFirma { get; set; }

    public string CadenaOriginal { get; set; }

    public Asunto? Asunto { get; set; }

    public int? AreaResponsable { get; set; }
  }
}
