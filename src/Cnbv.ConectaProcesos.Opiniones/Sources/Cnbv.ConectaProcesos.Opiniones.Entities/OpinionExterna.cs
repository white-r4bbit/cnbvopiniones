using System.ComponentModel.DataAnnotations;

namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
  public class OpinionExterna
  {
    public string IdEnvio { get; set; }

    public FirmaElectronica? FirmaElectronica { get; set; }
  }
}
