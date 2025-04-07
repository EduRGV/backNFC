using Microsoft.EntityFrameworkCore;
using NFC.Models;

namespace NFC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Profile> Profiles { get; set; }
    }
}
