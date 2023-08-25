using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Clients.UserClient
{
    public interface IUserServiceClient
    {
        Task<UserDto> GetUserAsync(string accessToken, Guid userId);
    }
}
