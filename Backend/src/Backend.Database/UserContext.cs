using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Database.Model;

namespace Backend.Database
{
    public class UserContext : DbContext
    {
        private DbContextOptions<UserContext> _options;
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
            _options = options;
           
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>()
                .Property(w => w.Id)
                .ValueGeneratedOnAdd();
        }
    }


}
