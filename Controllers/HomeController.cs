using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebGestionClientes.Models;
using WebGestionClientes.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace WebGestionClientes.Controllers
{
    public class HomeController : Controller
    {
        private readonly DBGestionClientesContext _DBContext;

        public HomeController(DBGestionClientesContext context)
        {
            _DBContext = context;
        }

        public IActionResult Index()
        {
            List<Cliente> listaClientes = _DBContext.Clientes.ToList();
            return View(listaClientes);
        }

        [HttpGet]
        public IActionResult ClienteDetalle(int idCliente)
        {
            ClienteVM clienteVM = new ClienteVM()
            {
                Cliente = new Cliente(),
            };

            if (idCliente > 0)
            {
                clienteVM.Cliente = _DBContext.Clientes.Find(idCliente);
            }

            return View(clienteVM);
        }

        [HttpPost]
        public IActionResult ClienteDetalle(ClienteVM clienteVM)
        {
            if (clienteVM.Cliente.Saldo <= 0)
            {
                TempData["ErrorMessage"] = "Error al crear el cliente: el saldo no puede ser negativo";

                return RedirectToAction("ClienteDetalle", "Home");
            }
            else
            {
                if (clienteVM.Cliente.ClienteId == 0)
                {
                    clienteVM.Cliente.Estado = "activo";
                    _DBContext.Clientes.Add(clienteVM.Cliente);
                }
                else
                {
                    clienteVM.Cliente.Estado = "activo";
                    _DBContext.Clientes.Update(clienteVM.Cliente);
                }

                _DBContext.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult Eliminar(int idCliente)
        {
            Cliente cliente = _DBContext.Clientes.Where(c => c.ClienteId == idCliente).FirstOrDefault();

            return View(cliente);
        }

        [HttpPost]
        public IActionResult Eliminar(Cliente cliente)
        {
            if (cliente.Saldo == 0)
            {
                cliente.Estado = "inactivo";

                _DBContext.Clientes.Update(cliente);
                _DBContext.SaveChanges();

                // Obtener la lista de clientes
                List<Cliente> listaClientes = _DBContext.Clientes.ToList();

                return View("Index", listaClientes);
            }
            else
            {
                TempData["ErrorMessage"] = "Error al eliminar, el cliente no debe tener saldo";
                return RedirectToAction("Index", "Home");
            }

        }

        [HttpGet]
        public IActionResult CuentaCorriente(int idCliente)
        {
            var movimientosCliente = _DBContext.CuentaCorrientes
                .Where(m => m.ClienteId == idCliente)
                .ToList();

            ViewData["idCliente"] = idCliente;

            return View(movimientosCliente);
        }


        [HttpGet]
        public IActionResult ClienteMovimiento(int idCliente)
        {

            var cliente = _DBContext.Clientes.FirstOrDefault(c => c.ClienteId == idCliente);

            //var viewModel = new MovimientoVM
            //{
            //    CuentaCorriente = new CuentaCorriente
            //    {
            //        Cliente = cliente
            //    }
            //};

            var vm = new MovimientoVM();
            vm.CuentaCorriente = new CuentaCorriente();
            vm.CuentaCorriente.Cliente = cliente;

            //decimal saldoCliente = _DBContext.Clientes
            //.Where(c => c.ClienteId == idCliente)
            //.Select(c => c.Saldo)
            //.FirstOrDefault();

            ViewBag.SaldoCliente = cliente.Saldo;
            return View(vm);
        }

        [HttpPost]
        public IActionResult ClienteMovimiento(string tipoMovimiento, int idCliente)
        {
            var cliente = _DBContext.Clientes.FirstOrDefault(c => c.ClienteId == idCliente);

            var viewModel = new MovimientoVM
            {
                TipoMovimiento = tipoMovimiento,
                CuentaCorriente = new CuentaCorriente
                {
                    Cliente = cliente,
                    ClienteId = idCliente
                }
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult CrearMovimiento(MovimientoVM movimientoVM, int clienteId)
        {
            try
            {
                string tipoMovimiento = movimientoVM.TipoMovimiento;
                var cliente = _DBContext.Clientes.FirstOrDefault(c => c.ClienteId == clienteId);
                movimientoVM.CuentaCorriente.Cliente = cliente;
                movimientoVM.CuentaCorriente.ClienteId = cliente.ClienteId;

                CuentaCorriente nm = new CuentaCorriente();
                nm.Descripcion = movimientoVM.CuentaCorriente.Descripcion;
                nm.Cliente = cliente;
                nm.ImporteDebito = movimientoVM.CuentaCorriente.ImporteDebito;
                nm.ImporteCredito = movimientoVM.CuentaCorriente.ImporteCredito;
                nm.FhMovimiento = DateTime.UtcNow;

                if (movimientoVM.CuentaCorriente.ImporteDebito > movimientoVM.CuentaCorriente.Cliente.Saldo)
                {
                    ModelState.AddModelError("", "No puede realizar un débito mayor que el saldo actual.");
                    return View("ClienteMovimiento", movimientoVM);
                }

                _DBContext.CuentaCorrientes.Add(nm);
                _DBContext.SaveChanges();

                if (movimientoVM.TipoMovimiento == "credito")
                {
                    cliente.Saldo += Convert.ToDecimal(movimientoVM.CuentaCorriente.ImporteCredito);

                }
                else if (movimientoVM.TipoMovimiento == "debito")
                {
                    cliente.Saldo -= Convert.ToDecimal(movimientoVM.CuentaCorriente.ImporteDebito);
                }
                _DBContext.SaveChanges();

                return RedirectToAction("CuentaCorriente", new { idCliente = movimientoVM.CuentaCorriente.ClienteId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar el movimiento: " + ex.Message);
            }

            return View(movimientoVM);
        }
    }
}