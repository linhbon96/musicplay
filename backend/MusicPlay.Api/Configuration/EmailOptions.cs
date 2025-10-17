namespace MusicPlay.Api.Configuration;

public class EmailOptions
{
    public string Provider { get; set; } = "SendGrid";
    public SendGridOptions SendGrid { get; set; } = new();
    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
}

public class SendGridOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = "no-reply@musicplay.local";
    public string SenderName { get; set; } = "MusicPlay";
}
