using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace OvBot.OwStats.DataBase
{
    class Context : DbContext
    {
        public DbSet<UsersBattleTag> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        { 
            options.UseSqlite("Data Source=owstat.db");
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UsersBattleTag>()
                .HasIndex(u => u.BattleTag)
                .IsUnique();
        }
    }
}
