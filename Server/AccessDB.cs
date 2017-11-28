using Common;
using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Server
{
    public class AccessDB: DbContext
    {
        public AccessDB() : base("dbConnection2015") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<User> ListOfUsers { get; set; }
        public virtual DbSet<Request> ListOfRequests { get; set; }
        public virtual DbSet<Team> ListOfTeams { get; set; }
    }
}
