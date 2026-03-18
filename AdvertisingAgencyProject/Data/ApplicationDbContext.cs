using AdvertisingAgencyProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingAgencyProject.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients => Set<Client>();
        public DbSet<AdCategory> AdCategories => Set<AdCategory>();
        public DbSet<AdOrder> AdOrders => Set<AdOrder>();
    }
}