using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Yuffie.WebApp.Models;

namespace WebApp.Migrations
{
    [DbContext(typeof(Yuffie.WebApp.Models.AppContext))]
    [Migration("20170822142906_Initals")]
    partial class Initals
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("Yuffie.WebApp.Models.Data", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("EntityId");

                    b.Property<string>("Key");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.HasIndex("EntityId");

                    b.ToTable("Data");
                });

            modelBuilder.Entity("Yuffie.WebApp.Models.Entity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.HasKey("Id");

                    b.ToTable("Entity");
                });

            modelBuilder.Entity("Yuffie.WebApp.Models.Data", b =>
                {
                    b.HasOne("Yuffie.WebApp.Models.Entity")
                        .WithMany("Data")
                        .HasForeignKey("EntityId");
                });
        }
    }
}
