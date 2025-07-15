using Microsoft.EntityFrameworkCore;

namespace BirthdayApp.Models
{
    public class BirthdayContext:DbContext
    {
        public BirthdayContext(DbContextOptions<BirthdayContext> options):base(options) 
        { 
        
        }
        public DbSet<UserList> Users { get; set; }
        public DbSet<BirthdayWish> BirthdayWishes { get; set; }
    }
}
