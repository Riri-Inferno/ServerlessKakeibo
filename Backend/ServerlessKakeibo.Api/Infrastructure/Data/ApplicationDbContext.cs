using Microsoft.EntityFrameworkCore;

namespace ServerlessKakeibo.Api.Infrastructure.Data
{
    /// <summary>
    /// ApplicationDbContext
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // public DbSet<ReceiptEntity> Receipts { get; set; }
    }
}
