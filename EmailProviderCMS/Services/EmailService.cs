using Azure;
using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using EmailProviderCMS.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace EmailProviderCMS.Services;

public class EmailService(ILogger<EmailService> logger, EmailClient emailClient)
{
    private readonly ILogger<EmailService> _logger = logger;
    private readonly EmailClient _emailClient = emailClient;

    public EmailRequest UnPackEmailRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var request = JsonConvert.DeserializeObject<EmailRequest>(message.Body.ToString()); 

            if(request != null)
            {
                return request;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.UnPackEmailRequest :: {ex.Message}");
        }

        return null!;
    }

    public bool SendEmail(EmailRequest emailRequest)
    {
        try
        {
            var result = _emailClient.Send(
                WaitUntil.Completed,
                senderAddress: Environment.GetEnvironmentVariable("SenderAddress"),
                recipientAddress: emailRequest.To,
                subject: emailRequest.Subject,
                htmlContent: emailRequest.HtmlBody,
                plainTextContent: emailRequest.PlainText
                );

            if (result.HasCompleted)
                return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.SendEmail :: {ex.Message}");
        }

        return false;
    }
}
