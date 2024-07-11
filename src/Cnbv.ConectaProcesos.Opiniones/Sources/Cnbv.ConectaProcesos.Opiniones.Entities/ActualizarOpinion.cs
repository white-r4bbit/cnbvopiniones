namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
    public class ActualizarOpinion
    {
        public int Id { get; set; }

        public string Comentarios { get; set; }

        public int SecuenciaFirma { get; set; }

        public string CadenaOriginal { get; set; }

        public ArchivosModelActualizar[] Archivos { get; set; }

        public ActualizarReceptor Receptor { get; set; }
    }
}