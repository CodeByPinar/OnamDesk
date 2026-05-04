using Microsoft.EntityFrameworkCore;
using OnamDesk.Data;
using System.IO;

namespace OnamDesk.Services
{
    public class DatabaseInitializerService
    {
        public async Task InitializeAsync()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "OnamDesk");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            using var context = new AppDbContext();

            await context.Database.MigrateAsync();
        }
    }
}