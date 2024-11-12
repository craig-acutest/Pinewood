using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pinewood.Api.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Logging.Abstractions;

namespace Pinewood.Api.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly ILogger<AuthController> _logger;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
                _userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null);
            _configurationMock = new Mock<IConfiguration>();
            _logger = new NullLogger<AuthController>();

            // Initialize the AuthController
            _authController = new AuthController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _configurationMock.Object,
                _logger);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "test@example.com", Password = "Password123" };
            _userManagerMock.Setup(um => um.FindByEmailAsync(loginRequest.Email)).ReturnsAsync((IdentityUser)null);

            // Act
            var result = await _authController.Login(loginRequest);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenPasswordCheckFails()
        {
            // Arrange
            var user = new IdentityUser { Email = "test@example.com" };
            var loginRequest = new LoginRequest { Email = user.Email, Password = "WrongPassword" };

            _userManagerMock.Setup(um => um.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _signInManagerMock.Setup(sm => sm.CheckPasswordSignInAsync(user, loginRequest.Password, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _authController.Login(loginRequest);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_ReturnsOk_WithToken_WhenLoginSuccessful()
        {
            // Arrange
            var user = new IdentityUser { Email = "test@example.com" };
            var loginRequest = new LoginRequest { Email = user.Email, Password = "Password123" };

            _userManagerMock.Setup(um => um.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _signInManagerMock.Setup(sm => sm.CheckPasswordSignInAsync(user, loginRequest.Password, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Mock GetRolesAsync to return an empty list or a list of roles
            _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "UserRole" });

            _configurationMock.Setup(c => c["Jwt:Key"]).Returns("YourVeryStrongSecretKeyThatIs32CharsOrLonger");
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("YourIssuer");
            _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("YourAudience");

            // Act
            var result = await _authController.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}
