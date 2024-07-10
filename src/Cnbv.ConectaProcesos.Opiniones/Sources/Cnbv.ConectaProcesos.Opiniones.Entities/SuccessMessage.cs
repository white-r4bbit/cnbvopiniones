namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
  public class SuccessMessage
  {
        private string v;

        public SuccessMessage(string v)
        {
            this.v = v;
        }

        public bool Success { get; set; }

    public string Message { get; set; }
  }
}
