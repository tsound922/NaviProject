using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviProject.Core.Interfaces
{
    public interface IAuthService
    {
        string GenerateToken(int userId, string username);
    }
}
