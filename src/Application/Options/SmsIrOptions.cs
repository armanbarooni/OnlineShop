// Moved to OnlineShop.Application.Settings to avoid name collision with Microsoft.Extensions.Options.Options
namespace OnlineShop.Application.Settings;

public class SmsIrSettings
{
    public string ApiKey { get; set; } = "lkU8Fckv7a9YD9mWJFLcO838S1lg2rKqElkIgipmQlYo3w8G";
    public int TemplateId { get; set; } = 1234567;
    public bool UseSandbox { get; set; } =false;
    public string OtpParamName { get; set; } = "Code"; 
}


