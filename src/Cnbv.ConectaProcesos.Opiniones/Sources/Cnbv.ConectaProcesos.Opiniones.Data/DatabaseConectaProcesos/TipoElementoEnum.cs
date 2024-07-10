using System;
using System.Collections.Generic;

namespace Cnbv.ConectaProcesos.Opiniones.Data.DatabaseConectaProcesos;

public partial class TipoElementoEnum
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<ArchivoOpinion> ArchivoOpinions { get; set; } = new List<ArchivoOpinion>();
}
