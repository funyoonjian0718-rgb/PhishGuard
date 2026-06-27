using Microsoft.EntityFrameworkCore;
using PhishGuard.Models;

namespace PhishGuard.Data
{
    public class PhishGuardDbContext : DbContext
    {
        public PhishGuardDbContext(DbContextOptions<PhishGuardDbContext> options)
            : base(options)
        {
        }

        public DbSet<EmailScanRecord> EmailScanRecords { get; set; }

        public DbSet<User> Users { get; set; }
    }
}