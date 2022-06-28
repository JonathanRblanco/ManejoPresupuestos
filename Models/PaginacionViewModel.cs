namespace ManejoPresupuestos.Models
{
    public class PaginacionViewModel
    {
        public int Pagina { get; set; } = 1;
        private int recordsPorPagina { get; set; } = 3;
        private readonly int cantidadaMaximaRecordsPorPagina = 50;
        public int RecordsPorPagina
        {
            get
            {
                return recordsPorPagina;
            }
            set
            {
                recordsPorPagina = value > cantidadaMaximaRecordsPorPagina ? cantidadaMaximaRecordsPorPagina : value;
            }
        }
        public int RecordsASaltar => recordsPorPagina * (Pagina - 1);
    }
}
