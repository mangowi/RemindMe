﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Database.Entity
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class RemindMeDbEntities : DbContext
    {
        public RemindMeDbEntities()
            : base("name=RemindMeDbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Reminder> Reminder { get; set; }
        public DbSet<Settings> Settings { get; set; }
        public DbSet<Songs> Songs { get; set; }
        public DbSet<PopupDimensions> PopupDimensions { get; set; }
        public DbSet<ListviewColumnSizes> ListviewColumnSizes { get; set; }
        public DbSet<Hotkeys> Hotkeys { get; set; }
        public DbSet<AdvancedReminderFilesFolders> AdvancedReminderFilesFolders { get; set; }
        public DbSet<AdvancedReminderProperties> AdvancedReminderProperties { get; set; }
        public DbSet<ReadMessages> ReadMessages { get; set; }
        public DbSet<ButtonSpaces> ButtonSpaces { get; set; }
        public DbSet<Themes> Themes { get; set; }
        public DbSet<HttpRequests> HttpRequests { get; set; }
        public DbSet<HttpRequestCondition> HttpRequestCondition { get; set; }
    }
}
