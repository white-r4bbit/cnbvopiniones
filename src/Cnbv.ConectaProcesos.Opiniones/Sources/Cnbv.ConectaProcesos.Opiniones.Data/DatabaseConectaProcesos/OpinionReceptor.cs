using System;
using System.Collections.Generic;

namespace Cnbv.ConectaProcesos.Opiniones.Data.DatabaseConectaProcesos;

public partial class OpinionReceptor
{
    public int Id { get; set; }

    public string? Clave { get; set; }

    public string? Comentarios { get; set; }

    public DateTime? FechaRespuesta { get; set; }

    public bool Obligatoria { get; set; }

    public bool Activa { get; set; }

    public int IdOpinion { get; set; }

    public bool Interno { get; set; }

    public string? FinalizadaPor { get; set; }

    public int? SecuenciaFirma { get; set; }

    public string? CadenaOriginal { get; set; }

    public string? IdEnvio { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<ArchivoOpinion> ArchivoOpinions { get; set; } = new List<ArchivoOpinion>();

    public virtual Opinion IdOpinionNavigation { get; set; } = null!;
}
