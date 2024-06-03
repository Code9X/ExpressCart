using System;
using System.Diagnostics;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ExpressCart.Utility
{
    public class Logging
    {
        public static void LogAction(string controllerName, string message, string userId,string? UserRole="")
        {
            var logFileName = $"{DateTime.Now.ToString("dd MMM yyyy")}.log";
            var logFilePath = Path.Combine("Logs", logFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
            string logMessage = $"{DateTime.Now.ToString("h:mm:ss tt")} - {message} ({controllerName}) | UserID:  {userId ?? "No User"}";

            using (var writer = new StreamWriter(logFilePath, append: true))
            {
                if (UserRole != "")
                    writer.WriteLine(logMessage + $" [{UserRole}]");
                else
                    writer.WriteLine(logMessage);
            }
        }
    }
} 
