using System;
using System.Collections.Generic;

namespace Cnbv.ConectaProcesos.Opiniones.Data.DatabaseConectaProcesos;

public partial class ArchivoOpinion
{
    public int Id { get; set; }

    public string? Ruta { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int IdTipoElemento { get; set; }

    public int IdTipoDocumento { get; set; }

    public int? IdOpinion { get; set; }

    public int? IdReceptor { get; set; }

    public string? Nombre { get; set; }

    public virtual Opinion? IdOpinionNavigation { get; set; }

    public virtual OpinionReceptor? IdReceptorNavigation { get; set; }

    public virtual TipoDocumentoEnum IdTipoDocumentoNavigation { get; set; } = null!;

    public virtual TipoElementoEnum IdTipoElementoNavigation { get; set; } = null!;

    public Boolean Eliminado { get; set; }
}
