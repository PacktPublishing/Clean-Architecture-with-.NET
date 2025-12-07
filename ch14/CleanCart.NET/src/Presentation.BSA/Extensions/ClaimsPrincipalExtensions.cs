using System.Security.Claims;

namespace Presentation.BSA.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetFirstName(this ClaimsPrincipal? user, string defaultValue = "Unknown")
    {
        return user?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? defaultValue;
    }

    public static string GetLastName(this ClaimsPrincipal? user, string defaultValue = "Unknown")
    {
        return user?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? defaultValue;
    }

    public static string GetEmail(this ClaimsPrincipal? user, string defaultValue = "")
    {
        return user?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? defaultValue;
    }

    public static string GetUserId(this ClaimsPrincipal? user, string defaultValue = "")
    {
        return user?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? defaultValue;
    }
}