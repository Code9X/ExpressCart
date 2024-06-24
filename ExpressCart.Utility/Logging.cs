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
        public static void LogAction(string controllerName, string message, string userId, string? UserRole = "")
        {
            var logFileName = $"{DateTime.Now:dd MMM yyyy}.log";
            var logDirectoryPath = Path.Combine("Logs", DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MMMM"));
            var logFilePath = Path.Combine(logDirectoryPath, logFileName);

            Directory.CreateDirectory(logDirectoryPath);

            string logMessage = $"{DateTime.Now:h:mm:ss tt} - {message} ({controllerName}) | UserID: {userId ?? "No User"}";

            using (var writer = new StreamWriter(logFilePath, append: true))
            {
                if (!string.IsNullOrEmpty(UserRole))
                    writer.WriteLine(logMessage + $" [{UserRole}]");
                else
                    writer.WriteLine(logMessage);
            }
        }
    }
} 
