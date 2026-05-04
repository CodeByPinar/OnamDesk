using System.IO;

namespace OnamDesk.Services
{
    public class BackupService
    {
        public string CreateDatabaseBackup()
        {
            var databasePath = GetDatabasePath();

            if (!File.Exists(databasePath))
            {
                throw new FileNotFoundException("Veritabanı dosyası bulunamadı.", databasePath);
            }

            var backupFolder = GetBackupFolder();

            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"onamdesk_backup_{timestamp}.db";
            var backupPath = Path.Combine(backupFolder, backupFileName);

            File.Copy(databasePath, backupPath, overwrite: false);

            return backupPath;
        }

        public List<string> GetBackupFiles()
        {
            var backupFolder = GetBackupFolder();

            if (!Directory.Exists(backupFolder))
            {
                return new List<string>();
            }

            return Directory
                .GetFiles(backupFolder, "onamdesk_backup_*.db")
                .OrderByDescending(x => x)
                .ToList();
        }

        public void DeleteOldBackups(int keepLastCount = 10)
        {
            var backupFiles = GetBackupFiles();

            if (backupFiles.Count <= keepLastCount)
            {
                return;
            }

            var filesToDelete = backupFiles
                .Skip(keepLastCount)
                .ToList();

            foreach (var file in filesToDelete)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // Eski yedek silinemezse uygulama kapanışını bozma.
                }
            }
        }

        private static string GetDatabasePath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dbFolder = Path.Combine(appDataPath, "OnamDesk");

            return Path.Combine(dbFolder, "onamdesk.db");
        }

        private static string GetBackupFolder()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            return Path.Combine(documentsPath, "OnamDesk", "Backups");
        }
    }
}