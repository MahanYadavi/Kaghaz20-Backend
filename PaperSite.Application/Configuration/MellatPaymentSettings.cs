namespace PaperSite.Application.Configuration;

public class MellatPaymentSettings
{
    public long TerminalId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserPassword { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
    public string WebServiceUrl { get; set; } = "https://bpm.shaparak.ir/pgwchannel/services/pgw";
    public string PaymentPageUrl { get; set; } = "https://bpm.shaparak.ir/pgwchannel/startpay.mellat";
    public string FrontendResultUrl { get; set; } = "https://www.kaghaz20.ir/payment/result";
}
