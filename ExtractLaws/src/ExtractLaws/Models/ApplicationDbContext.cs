using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Storage;
using ExtractLaws.Entities;

namespace ExtractLaws.Models
{
    public class ApplicationDbContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        public DbSet<Law> Laws { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder)
        //{

        //    var connString = Startup.Configuration["Data:WorldContextConnection"];
        //    optionBuilder.UseSqlServer(connString);

        //    base.OnConfiguring(optionBuilder);
        //}
    }
}
