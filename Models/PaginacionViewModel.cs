namespace ManejoPresupuesto.Models
{
    public class PaginacionViewModel
    {
        public int Pagina { get; set; } = 1;
        private int recordsPorPagina = 10;
        private readonly int _cantidadMaximaRecodsPorPagina = 50;

        public int RecordsPorPagina
        {
            get
            {
                return recordsPorPagina;
            }
            set
            {
                recordsPorPagina = (value > _cantidadMaximaRecodsPorPagina) ? _cantidadMaximaRecodsPorPagina : value;
            }
        }

        public int RecordsASaltar => recordsPorPagina * (Pagina - 1);
    }
}
