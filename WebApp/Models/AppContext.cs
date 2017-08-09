using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Yuffie.WebApp.Models {
public class AppContext : DbContext
{
      public AppContext(DbContextOptions<AppContext> options)
          : base(options)
      { }
       public DbSet<Entity> Entity { get; set; }
    }
}