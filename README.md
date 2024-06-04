**1.Add appsettings.json to this project.**

{
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "AllowedHosts": "*",
    "ConnectionStrings": {
        "DefaultConnection": "Server=<SERV_NAME>;Database=<DB_NAME>;Trusted_Connection=True;TrustServerCertificate=True",
        "ApplicationDbContextConnection": "Server=(localdb)\\mssqllocaldb;Database=<DB_NAME>;Trusted_Connection=True;MultipleActiveResultSets=true"
    },
    "PaymentSettings": {
        "SecretKey": "KEY_SECRET",
        "PublishableKey": "KEY_PUBLISH"
    }
}

**2.Run EF Migration :**
add-migration <MigrationName> ,
update-database

**3.ADMIN LOGIN :**
UserName : Admin@ec.com
Password : Admin123#
