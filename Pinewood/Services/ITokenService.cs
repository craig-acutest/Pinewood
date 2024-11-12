using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pinewood.Models;

namespace Pinewood.Services
{
    public interface ITokenService
    {
        Task<string> LoginAsync(LoginRequest loginRequest);

        string? GetToken(string email);

        void DeleteToken(string email);

        Task<bool> LoggedIn(string email);
    }
}
