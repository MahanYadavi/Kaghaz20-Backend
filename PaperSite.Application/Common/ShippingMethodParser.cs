using PaperSite.Domain.Enums;

namespace PaperSite.Application.Common;

/// <summary>
/// تبدیل رشته روش ارسال دریافتی از فرانت به <see cref="ShippingMethod"/>.
/// </summary>
public static class ShippingMethodParser
{
    public const string ExpressValue = "express";
    public const string TipaxValue = "tipax";

    public static bool TryParse(string? value, out ShippingMethod shippingMethod)
    {
        shippingMethod = default;
        if (string.IsNullOrWhiteSpace(value)) return false;

        switch (value.Trim().ToLowerInvariant())
        {
            case ExpressValue:
                shippingMethod = ShippingMethod.Express;
                return true;
            case TipaxValue:
                shippingMethod = ShippingMethod.Tipax;
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// نمایش رشته‌ای روش ارسال برای پاسخ API (همان مقادیری که فرانت می‌فرستد).
    /// </summary>
    public static string ToApiValue(ShippingMethod shippingMethod) => shippingMethod switch
    {
        ShippingMethod.Express => ExpressValue,
        ShippingMethod.Tipax => TipaxValue,
        _ => shippingMethod.ToString().ToLowerInvariant()
    };
}