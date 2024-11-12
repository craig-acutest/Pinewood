using Microsoft.AspNetCore.Mvc;
using Pinewood.Models;
using Pinewood.Services;

namespace Pinewood.Controllers
{
    [CustomAuthorize("Admin")]
    public class CustomerController : Controller
    {
        private readonly CustomerApiClient _customerApiClient;
        private readonly AuthApiClient _authApiClient;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(CustomerApiClient customerApiClient, AuthApiClient authApiClient, ILogger<CustomerController> logger)
        {
            _customerApiClient = customerApiClient;
            _authApiClient = authApiClient;
            _logger = logger;            
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _customerApiClient.GetCustomersAsync();

            return View(customers);
        }

        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerApiClient.GetCustomerByIdAsync(id);
            return View(customer);
        }

        public IActionResult Create()
        {
            return View();
        }

        // POST: Customer/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,Phone")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _customerApiClient.CreateCustomerAsync(customer);
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _customerApiClient.GetCustomerByIdAsync(id.Value);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customer/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Phone")] Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _customerApiClient.UpdateCustomerAsync(id, customer);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while updating the customer.");
                    throw;
                }                
            }
            return View(customer);
        }

        // GET: Customer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _customerApiClient.GetCustomerByIdAsync(id.Value);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _customerApiClient.GetCustomerByIdAsync(id);
            if (customer != null)
            {
                await _customerApiClient.DeleteCustomerAsync(id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
