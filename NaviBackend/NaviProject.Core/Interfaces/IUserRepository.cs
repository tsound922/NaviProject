using NaviProject.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviProject.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<AppUser?> GetByUsernameAsync(string username);
        Task<AppUser?> GetByEmailAsync(string email);
        Task<int> CreateUserAsync(string username, string email, string passwordHash);
    }
}
