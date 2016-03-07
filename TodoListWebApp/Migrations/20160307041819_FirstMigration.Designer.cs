using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using TodoListWebApp.Models;

namespace TodoListWebApp.Migrations
{
    [DbContext(typeof(TodoListWebAppContext))]
    [Migration("20160307041819_FirstMigration")]
    partial class FirstMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("TodoListWebApp.Models.AADUserRecord", b =>
                {
                    b.Property<string>("UPN");

                    b.Property<string>("TenantID");

                    b.HasKey("UPN");
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
                });

            modelBuilder.Entity("TodoListWebApp.Models.Todo", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Owner");

                    b.HasKey("ID");
                });

            modelBuilder.Entity("TodoListWebApp.Utils.PerWebUserCache", b =>
                {
                    b.Property<int>("EntryId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("LastWrite");

                    b.Property<byte[]>("cacheBits");

                    b.Property<string>("webUserUniqueId");

                    b.HasKey("EntryId");
                });
        }
    }
}
