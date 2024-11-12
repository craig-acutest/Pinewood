using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Pinewood.Api.Controllers;
using Pinewood.Api.Data;
using Pinewood.Api.Models;

namespace Pinewood.Api.Tests.Controllers
{
    public class CustomersControllerTests
    {
        private readonly CustomersController _controller;
        private readonly ApplicationDbContext _context;
        public CustomersControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new CustomersController(_context);

            // Clear the database before each test
            _context.Customers.RemoveRange(_context.Customers);
            _context.SaveChanges();

            // Seed the database with fresh data
            _context.Customers.AddRange(
                new Customer { Id = 1, Name = "John Doe", Email = "john.doe@test.com", Phone = "07917195800" },
                new Customer { Id = 2, Name = "Jane Doe", Email = "jane.doe@test.com", Phone = "07917195801" }
            );
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetCustomers_ReturnsAllCustomers()
        {
            // Act
            var result = await _controller.GetCustomers();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Customer>>>(result);
            var customers = Assert.IsType<List<Customer>>(actionResult.Value);
            Assert.Equal(2, customers.Count);
        }

        [Fact]
        public async Task GetCustomer_ReturnsCustomer_WhenCustomerExists()
        {
            // Act
            var result = await _controller.GetCustomer(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Customer>>(result);
            var customer = Assert.IsType<Customer>(actionResult.Value);
            Assert.Equal("John Doe", customer.Name);
        }

        [Fact]
        public async Task GetCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Act
            var result = await _controller.GetCustomer(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PutCustomer_ReturnsNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange
            var updatedCustomer = new Customer { Id = 1, Name = "Updated Name", Email = "test@test.com", Phone = "07917195803" };

            // Detach the existing entity from the context
            var existingCustomer = await _context.Customers.FindAsync(1);
            _context.Entry(existingCustomer).State = EntityState.Detached;

            // Act
            var result = await _controller.PutCustomer(1, updatedCustomer);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var customer = await _context.Customers.FindAsync(1);
            Assert.Equal("Updated Name", customer.Name);
        }

        [Fact]
        public async Task PutCustomer_ReturnsBadRequest_WhenIdsDoNotMatch()
        {
            // Arrange
            var updatedCustomer = new Customer { Id = 2, Name = "Updated Name" };

            // Act
            var result = await _controller.PutCustomer(1, updatedCustomer);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task PostCustomer_ReturnsCreatedAtAction_WhenCustomerIsCreated()
        {
            // Arrange
            var newCustomer = new Customer { Id = 3, Name = "New Customer", Email = "testcreate@test.com", Phone = "07917195804" };

            // Act
            var result = await _controller.PostCustomer(newCustomer);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Customer>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var customer = Assert.IsType<Customer>(createdAtActionResult.Value);
            Assert.Equal("New Customer", customer.Name);
            Assert.Equal(3, customer.Id);
        }

        [Fact]
        public async Task DeleteCustomer_ReturnsNoContent_WhenCustomerIsDeleted()
        {
            // Act
            var result = await _controller.DeleteCustomer(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var customer = await _context.Customers.FindAsync(1);
            Assert.Null(customer);
        }

        [Fact]
        public async Task DeleteCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Act
            var result = await _controller.DeleteCustomer(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}

