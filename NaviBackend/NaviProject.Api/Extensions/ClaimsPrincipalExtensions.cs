using System.Security.Claims;

namespace NaviProject.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) throw new UnauthorizedAccessException("User ID not found in token.");
        return int.Parse(claim.Value);
    }
}