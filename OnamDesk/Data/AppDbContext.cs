using Microsoft.EntityFrameworkCore;
using OnamDesk.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnamDesk.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Template> Templates => Set<Template>();
        public DbSet<ConsentForm> ConsentForms => Set<ConsentForm>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dbFolder = Path.Combine(appDataPath, "OnamDesk");

            if (!Directory.Exists(dbFolder))
            {
                Directory.CreateDirectory(dbFolder);
            }

            var dbPath = Path.Combine(dbFolder, "onamdesk.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.FullName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.TcNoEncrypted)
                    .IsRequired();

                entity.Property(x => x.Phone)
                    .HasMaxLength(30);

                entity.Property(x => x.CreatedAt)
                    .IsRequired();
            });

            modelBuilder.Entity<Template>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Category)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.ContentJson)
                    .IsRequired();

                entity.Property(x => x.Risks)
                    .IsRequired();

                entity.Property(x => x.IsActive)
                    .IsRequired();

                entity.Property(x => x.UpdatedAt)
                    .IsRequired();
            });

            modelBuilder.Entity<ConsentForm>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Patient)
                    .WithMany(x => x.ConsentForms)
                    .HasForeignKey(x => x.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Template)
                    .WithMany(x => x.ConsentForms)
                    .HasForeignKey(x => x.TemplateId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(x => x.SignatureData)
                    .IsRequired();

                entity.Property(x => x.SignatureHash)
                    .IsRequired();

                entity.Property(x => x.SignedAt)
                    .IsRequired();

                entity.Property(x => x.PdfPath)
                    .IsRequired();

                entity.Property(x => x.DoctorName)
                    .IsRequired()
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.ConsentForm)
                    .WithMany(x => x.AuditLogs)
                    .HasForeignKey(x => x.ConsentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Property(x => x.Action)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.UserName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.Timestamp)
                    .IsRequired();
            });
        }
    }
}
