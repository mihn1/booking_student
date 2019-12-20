using B2S.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace B2S.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // PROFILE ROLE
            builder.Entity("B2S.Core.Entities.ProfileRole", b =>
            {
                b.Property<string>("UserProfileId");

                b.Property<string>("RoleId");

                b.HasKey("UserProfileId", "RoleId");

                b.HasIndex("RoleId");

                b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId");

                b.HasOne("B2S.Core.Entities.UserProfile", "UserProfile")
                    .WithMany()
                    .HasForeignKey("UserProfileId");

                b.ToTable("ProfileRole");
            });

            builder.Entity<UserAccount>().HasKey(a => new
            {
                a.AccountId,
                a.UserId
            });
        }

        
        public DbSet<User> User { get; set; }
        public DbSet<ProfileRole> ProfileRole { get; set; }
        public DbSet<UserProfile> UserProfile { get; set; }
        public DbSet<UserAccount> UserAccount { get; set; }
        public DbSet<Bed> Bed { get; set; }
        public DbSet<Building> Building { get; set; }
        public DbSet<Property> Property { get; set; }
        public DbSet<BuildingImage> BuildingImage { get; set; }
        public DbSet<Room> Room { get; set; }
        public DbSet<RoomType> RoomType { get; set; }
        public DbSet<Agent> Agent { get; set; }
        public DbSet<Booking> Booking { get; set; }
        public DbSet<BookingNote> BookingNote { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Vendor> Vendor { get; set; }
        public DbSet<Invoice> Invoice { get; set; }
        public DbSet<InvoiceItem> InvoiceItem { get; set; }
        public DbSet<InvoiceSetting> InvoiceSetting { get; set; }
        public DbSet<BookingData> BookingData { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<EmailTemplate> EmailTemplate { get; set; }
        public DbSet<Expense> Expense { get; set; }
        public DbSet<ExpenseCategory> ExpenseCategory { get; set; }
        public DbSet<DamageReport> DamageReport { get; set; }
        public DbSet<PropertyDocument> PropertyDocument { get; set; }

    }
}
