namespace PaperSite.Application.Common;

/// <summary>
/// کمک‌کننده‌های متن فارسی/عربی برای اعتبارسنجی ورودی کاربر.
/// </summary>
public static class TextNormalizer
{
    /// <summary>
    /// تبدیل ارقام فارسی/عربی به ارقام انگلیسی.
    /// </summary>
    public static string? NormalizeDigits(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var buffer = new char[value.Length];
        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];
            buffer[i] = character switch
            {
                >= '\u06F0' and <= '\u06F9' => (char)(character - '\u06F0' + '0'), // ۰-۹
                >= '\u0660' and <= '\u0669' => (char)(character - '\u0660' + '0'), // ٠-٩
                _ => character
            };
        }

        return new string(buffer);
    }

    /// <summary>
    /// تبدیل ارقام فارسی/عربی به انگلیسی و حذف فاصله‌ها و خط تیره‌ها.
    /// </summary>
    public static string? NormalizeDigitOnly(string? value)
    {
        var normalized = NormalizeDigits(value);
        if (string.IsNullOrEmpty(normalized)) return normalized;

        return new string(normalized.Where(character => !char.IsWhiteSpace(character) && character != '-').ToArray());
    }
}