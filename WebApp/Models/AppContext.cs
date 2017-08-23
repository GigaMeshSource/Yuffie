using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Yuffie.WebApp.Models {
public class AppContext : DbContext
{
    //   public AppContext(DbContextOptions<AppContext> options) : base(options)
    //   { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=MyDatabase;Trusted_Connection=True;");
            optionsBuilder.UseSqlite("Filename=./confWebApp.db");
        }


       public DbSet<Entity> Entity { get; set; }
    }
}