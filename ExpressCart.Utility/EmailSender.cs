using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ExpressCart.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email,string subject,string htmlMessage)
        {
            //Add Logic To send  Email Later
            return Task.CompletedTask;
        }
    }
}
