using NaviProject.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviProject.Core.Interfaces
{
    public interface ILanguageModelService
    {
        Task<string> CompleteAsync(string prompt, IEnumerable<ChatMessage> history);
    }
}
