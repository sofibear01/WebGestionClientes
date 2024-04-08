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
                TempData["ErrorMessage"] = "Error al eliminar el usuario: el saldo debe ser 0";
                return RedirectToAction("Index", "Home");
            }

        }

        [HttpGet]
        public IActionResult CuentaCorriente(int idCliente)
        {
            var movimientosCliente = _DBContext.CuentaCorrientes
                .Where(m => m.ClienteId == idCliente)
                .ToList();

            // Calcular el saldo del cliente
            decimal saldoCliente = Convert.ToDecimal(movimientosCliente.Sum(m => Convert.ToDecimal(m.ImporteCredito))) - Convert.ToDecimal(movimientosCliente.Sum(m => Convert.ToDecimal(m.ImporteDebito)));

            ViewData["idCliente"] = idCliente;
            ViewBag.SaldoCliente = saldoCliente; // Opcional: utilizando ViewBag

            return View(movimientosCliente);
        }


        [HttpGet]
        public IActionResult ClienteMovimiento(int idCliente)
        {
            var viewModel = new MovimientoVM
            {
                CuentaCorriente = new CuentaCorriente
                {
                    ClienteId = idCliente
                }
            };

            decimal saldoCliente = _DBContext.Clientes
            .Where(c => c.ClienteId == idCliente)
            .Select(c => c.Saldo)
            .FirstOrDefault();

            // Guardar el saldo en el ViewBag
            ViewBag.SaldoCliente = saldoCliente;
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult ClienteMovimiento(string tipoMovimiento, int idCliente)
        {
            var viewModel = new MovimientoVM
            {
                TipoMovimiento = tipoMovimiento,
                CuentaCorriente = new CuentaCorriente
                {
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
                movimientoVM.CuentaCorriente.ClienteId = clienteId;
                movimientoVM.CuentaCorriente.Cliente = _DBContext.Clientes.FirstOrDefault(c => c.ClienteId == clienteId);

                // Crear un nuevo objeto de Movimiento con los datos del formulario
                CuentaCorriente nm = new CuentaCorriente();
                nm.Descripcion = movimientoVM.CuentaCorriente.Descripcion;
                nm.Cliente = movimientoVM.CuentaCorriente.Cliente;
                nm.ImporteDebito = movimientoVM.CuentaCorriente.ImporteDebito;
                nm.ImporteCredito = movimientoVM.CuentaCorriente.ImporteCredito;
                nm.FhMovimiento = DateTime.Now;

                if (movimientoVM.CuentaCorriente.ImporteDebito > movimientoVM.CuentaCorriente.Cliente.Saldo)
                {
                    ModelState.AddModelError("", "No puede realizar un débito mayor que su saldo actual.");
                    // Redirigir de vuelta a la vista ClienteMovimiento con el modelo actual
                    return View("ClienteMovimiento", movimientoVM);
                }

                _DBContext.CuentaCorrientes.Add(nm);
                _DBContext.SaveChanges();

                // Actualizar el saldo del cliente según el tipo de movimiento
                Cliente cliente = _DBContext.Clientes.FirstOrDefault(c => c.ClienteId == movimientoVM.CuentaCorriente.ClienteId);

                if (movimientoVM.TipoMovimiento == "credito")
                {
                    cliente.Saldo += Convert.ToDecimal(movimientoVM.CuentaCorriente.ImporteCredito);

                }
                else if (movimientoVM.TipoMovimiento == "debito")
                {
                    cliente.Saldo -= Convert.ToDecimal(movimientoVM.CuentaCorriente.ImporteDebito);
                }
                _DBContext.SaveChanges();


                // Redirigir a la acción CuentaCorriente con el ID del cliente
                return RedirectToAction("CuentaCorriente", new { idCliente = movimientoVM.CuentaCorriente.ClienteId });
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción aquí
                ModelState.AddModelError("", "Error al guardar el movimiento: " + ex.Message);
            }

            // Si hay un error, devolver la vista con el modelo para mostrar los mensajes de error
            return View(movimientoVM);
        }
    }
}