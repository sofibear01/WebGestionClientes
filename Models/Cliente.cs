using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebGestionClientes.Models
{
    public partial class Cliente
    {
        public Cliente()
        {
        }

        public int ClienteId { get; set; }
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public decimal Saldo { get; set; }
        public string? Estado { get; set; }
        public virtual CuentaCorriente CuentaCorriente { get; set; }
    }
}
