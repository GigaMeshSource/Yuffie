using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Yuffie.WebApp.Models {
public class AppContext : DbContext
{
      public AppContext(DbContextOptions<AppContext> options) : base(options)
      { 
      }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=tcp:anime-co-db.database.windows.net,1433;Initial Catalog=yuffie-anim;Persist Security Info=False;User ID=azureworker;Password=Tennis94;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30");
            // optionsBuilder.UseSqlite("Filename=./confWebApp.db");
        }


       public DbSet<Entity> Entity { get; set; }
    }
}