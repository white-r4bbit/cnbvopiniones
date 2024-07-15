namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
  public class ActualizarReceptor
  { 
    public int id { get; set; }
    public string? Firmante { get; set; }

    public string? EstatusSolicitud { get; set; }

    public string? ComentarioFirmante { get; set; }

    public ArchivosModelActualizar[]? Archivos { get; set; }

  }
}