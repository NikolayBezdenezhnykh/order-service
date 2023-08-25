using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Clients.AuthService
{
    public interface IAuthServiceClient
    {
        Task<AuthResponse> GetTokenAsync(IReadOnlyList<string> resources);
    }
}
