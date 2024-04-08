using System;
using System.Collections.Generic;

namespace WebGestionClientes.Models
{
    public partial class CuentaCorriente
    {
        public int MovimientoId { get; set; }
        public int? ClienteId { get; set; }
        public DateTime FhMovimiento { get; set; }
        public decimal? ImporteCredito { get; set; } 
        public decimal? ImporteDebito { get; set; } 
        public string Descripcion { get; set; } = null!;
        public virtual Cliente Cliente { get; set; }

    }
}
