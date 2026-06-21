using Microsoft.EntityFrameworkCore;
using SareeProductionApp.Client.Models;

namespace SareeProductionApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<JobWorker> JobWorkers => Set<JobWorker>();
        public DbSet<YarnMaster> YarnMasters => Set<YarnMaster>();
        public DbSet<YarnStock> YarnStocks => Set<YarnStock>();
        public DbSet<DyeingYarnDelivery> DyeingYarnDeliveries => Set<DyeingYarnDelivery>();
        public DbSet<DyeingYarnDeliveryItem> DyeingYarnDeliveryItems => Set<DyeingYarnDeliveryItem>();
        public DbSet<DyeingYarnReceipt> DyeingYarnReceipts => Set<DyeingYarnReceipt>();
        public DbSet<DyeingYarnReceiptItem> DyeingYarnReceiptItems => Set<DyeingYarnReceiptItem>();
        public DbSet<WarpBeamMaster> WarpBeamMasters => Set<WarpBeamMaster>();
        public DbSet<WeaverIssue> WeaverIssues => Set<WeaverIssue>();
        public DbSet<WeaverIssueItem> WeaverIssueItems => Set<WeaverIssueItem>();
        public DbSet<SareeReceipt> SareeReceipts => Set<SareeReceipt>();
        public DbSet<SareeReceiptItem> SareeReceiptItems => Set<SareeReceiptItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure default schemas (Supabase public schema)
            modelBuilder.HasDefaultSchema("public");

            // Configure Global Query Filters for Soft Deletes (deleted_at IS NULL)
            modelBuilder.Entity<JobWorker>().HasQueryFilter(w => w.DeletedAt == null);
            modelBuilder.Entity<YarnMaster>().HasQueryFilter(y => y.DeletedAt == null);
            modelBuilder.Entity<YarnStock>().HasQueryFilter(s => s.DeletedAt == null);
            modelBuilder.Entity<DyeingYarnDelivery>().HasQueryFilter(d => d.DeletedAt == null);
            modelBuilder.Entity<DyeingYarnDeliveryItem>().HasQueryFilter(di => di.DeletedAt == null);
            modelBuilder.Entity<DyeingYarnReceipt>().HasQueryFilter(r => r.DeletedAt == null);
            modelBuilder.Entity<DyeingYarnReceiptItem>().HasQueryFilter(ri => ri.DeletedAt == null);
            modelBuilder.Entity<WarpBeamMaster>().HasQueryFilter(b => b.DeletedAt == null);
            modelBuilder.Entity<WeaverIssue>().HasQueryFilter(i => i.DeletedAt == null);
            modelBuilder.Entity<WeaverIssueItem>().HasQueryFilter(ii => ii.DeletedAt == null);
            modelBuilder.Entity<SareeReceipt>().HasQueryFilter(sr => sr.DeletedAt == null);
            modelBuilder.Entity<SareeReceiptItem>().HasQueryFilter(sri => sri.DeletedAt == null);

            // Cascade behaviors and relational constraints
            modelBuilder.Entity<DyeingYarnDeliveryItem>()
                .HasOne<DyeingYarnDelivery>()
                .WithMany(d => d.Items)
                .HasForeignKey(di => di.DeliveryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DyeingYarnReceiptItem>()
                .HasOne<DyeingYarnReceipt>()
                .WithMany(r => r.Items)
                .HasForeignKey(ri => ri.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WeaverIssueItem>()
                .HasOne<WeaverIssue>()
                .WithMany(i => i.Items)
                .HasForeignKey(ii => ii.WeaverIssueId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SareeReceiptItem>()
                .HasOne<SareeReceipt>()
                .WithMany(r => r.Items)
                .HasForeignKey(sri => sri.SareeReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
