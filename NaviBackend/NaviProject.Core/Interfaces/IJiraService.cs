using NaviProject.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviProject.Core.Interfaces
{
    public interface IJiraService
    {
        Task<List<JiraTicket>> GetMyTicketsAsync(int maxResults = 100);
        Task SyncToKnowledgeBaseAsync(int userId, bool isPublic = false);
    }
}
