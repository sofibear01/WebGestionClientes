namespace WebGestionClientes.Models.ViewModels
{
    public class MovimientoVM
    {
        public CuentaCorriente CuentaCorriente { get; set; }
        public string TipoMovimiento { get; set; }

        public MovimientoVM()
        {
            TipoMovimiento = null;
        }
    }


}
