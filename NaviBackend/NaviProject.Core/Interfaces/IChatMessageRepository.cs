using NaviProject.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviProject.Core.Interfaces
{
    public interface IChatMessageRepository
    {
        Task AddMessageAsync(int chatId, string role, string content);
        Task<IEnumerable<ChatMessage>> GetMessagesByChatIdAsync(int chatId);
    }
}
