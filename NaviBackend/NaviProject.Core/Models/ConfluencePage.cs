using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviProject.Core.Models;

public class ConfluencePage
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string SpaceKey { get; set; } = string.Empty;
    public string SpaceName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string LastModifiedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
}
