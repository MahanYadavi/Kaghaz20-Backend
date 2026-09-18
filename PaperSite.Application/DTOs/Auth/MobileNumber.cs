namespace PaperSite.Application.DTOs.Auth;

public static class MobileNumber
{
    public static string Normalize(string? value)
    {
        var mobile = new string((value ?? "").Trim().Select(c => c switch
        {
            >= '۰' and <= '۹' => (char)('0' + c - '۰'),
            >= '٠' and <= '٩' => (char)('0' + c - '٠'),
            _ => c
        }).ToArray()).Replace(" ", "").Replace("-", "");
        if (mobile.StartsWith("+98")) mobile = "0" + mobile[3..];
        else if (mobile.StartsWith("0098")) mobile = "0" + mobile[4..];
        else if (mobile.StartsWith("98") && mobile.Length == 12) mobile = "0" + mobile[2..];
        return mobile;
    }
}
