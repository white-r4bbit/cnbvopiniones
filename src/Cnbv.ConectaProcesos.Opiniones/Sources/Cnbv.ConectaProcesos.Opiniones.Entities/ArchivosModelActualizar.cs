namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
  public class ArchivosModelActualizar
  {
    public int id { get; set; }
    public string Ruta { get; set; }

    public string Nombre { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public string TipoElemento { get; set; }

    public string TipoDocumento { get; set; }

    public Boolean eliminado { get; set; }
  }
}