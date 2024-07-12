using System.Data.Common;

namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
    public class ArchivosModelResponse
    {   
        public int Id  { get; set; }
        
        public string Ruta { get; set; }

        public DateTime FechaCreacion { get; set; }

        public string Nombre { get; set; }

        public string TipoElemento { get; set; }

        public string TipoDocumento { get; set; }
    }
}