using System.Security.Claims;

namespace Presentation.BSA.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetFirstName(this ClaimsPrincipal? user, string defaultValue = "Unknown")
    {
        return user?.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value ?? defaultValue;
    }

    public static string GetLastName(this ClaimsPrincipal? user, string defaultValue = "Unknown")
    {
        return user?.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")?.Value ?? defaultValue;
    }

    public static string GetEmail(this ClaimsPrincipal? user, string defaultValue = "")
    {
        return user?.Claims.FirstOrDefault(c => c.Type == "emails")?.Value ?? defaultValue;
    }

    public static string GetUserId(this ClaimsPrincipal? user, string defaultValue = "")
    {
        return user?.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ?? defaultValue;
    }
}

