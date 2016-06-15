using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using TodoListWebApp.Models;

namespace TodoListWebApp.Migrations
{
    [DbContext(typeof(TodoListWebAppContext))]
    [Migration("20160614191241_initial")]
    partial class initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rc2-20896");

            modelBuilder.Entity("TodoListWebApp.Models.AADUserRecord", b =>
                {
                    b.Property<string>("ObjectID");

                    b.Property<string>("UPN");

                    b.HasKey("ObjectID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("TodoListWebApp.Models.Tenant", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("AdminConsented");

                    b.Property<DateTime>("Created");

                    b.Property<string>("IssValue");

                    b.Property<string>("Name");

                    b.HasKey("ID");

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("TodoListWebApp.Models.Todo", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Owner");

                    b.HasKey("ID");

                    b.ToTable("Todoes");
                });

            modelBuilder.Entity("TodoListWebApp.Services.PerWebUserCache", b =>
                {
                    b.Property<int>("EntryId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("LastWrite");

                    b.Property<byte[]>("cacheBits");

                    b.Property<string>("webUserUniqueId");

                    b.HasKey("EntryId");

                    b.ToTable("PerUserCacheList");
                });
        }
    }
}
