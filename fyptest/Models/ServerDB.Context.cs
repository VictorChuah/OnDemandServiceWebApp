﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace fyptest.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ServerDBEntities : DbContext
    {
        public ServerDBEntities()
            : base("name=ServerDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<Chat> Chats { get; set; }
        public virtual DbSet<ChatConnection> ChatConnections { get; set; }
        public virtual DbSet<Document> Documents { get; set; }
        public virtual DbSet<Provider> Providers { get; set; }
        public virtual DbSet<Rating> Ratings { get; set; }
        public virtual DbSet<Request> Requests { get; set; }
        public virtual DbSet<Seeker> Seekers { get; set; }
        public virtual DbSet<Service_Category> Service_Categories { get; set; }
        public virtual DbSet<Service_Type> Service_Types { get; set; }
    }
}
