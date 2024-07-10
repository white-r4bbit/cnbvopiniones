using System;
using System.Collections.Generic;

namespace Cnbv.ConectaProcesos.Opiniones.Data.DatabaseConectaProcesos;

public partial class Opinion
{
    public int Id { get; set; }

    public string FolioAsunto { get; set; } = null!;

    public DateTime FechaSolicitud { get; set; }

    public string Detalle { get; set; } = null!;

    public bool Activa { get; set; }

    public int Version { get; set; }

    public int SecuenciaFirma { get; set; }

    public string CadenaOriginal { get; set; } = null!;

    public virtual ICollection<ArchivoOpinion> ArchivoOpinions { get; set; } = new List<ArchivoOpinion>();

    public virtual ICollection<OpinionReceptor> OpinionReceptors { get; set; } = new List<OpinionReceptor>();
}
