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
        Task<int> CreateChatAsync(string? title, int userId);
        Task<Chat?> GetChatByIdAsync(int chatId, int userId);
        Task<IEnumerable<Chat>> GetAllChatsAsync(int userId);
        Task UpdateChatTitleAsync(int chatId, string title);
        Task DeleteChatAsync(int chatId, int userId);
    }
}
