using NaviProject.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviProject.Core.Interfaces
{
    public interface IChatRepository
    {
        Task<int> CreateChatAsync(string? title);
        Task<Chat?> GetChatByIdAsync(int chatId);
        Task<IEnumerable<Chat>> GetAllChatsAsync();
        Task UpdateChatTitleAsync(int chatId, string title);
        Task DeleteChatAsync(int chatId);
    }
}
