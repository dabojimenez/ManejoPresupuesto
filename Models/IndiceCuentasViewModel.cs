namespace ManejoPresupuesto.Models
{
    public class IndiceCuentasViewModel
    {
        public string TipoCuenta { get; set; }
        public IEnumerable<Cuenta> Cuentas { get; set; }
        //este atributa sumara los balances, de las cuentas, pertenecientes al tipocuenta
        public decimal Balance => Cuentas.Sum(c => c.Balance);
    }
}
