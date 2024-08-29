using Microsoft.EntityFrameworkCore;

namespace ApiForLongRunningTasks;

public class StoreDbContext : DbContext
{
    public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; } 
}