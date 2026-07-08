using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Models;

namespace PropertyBill.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Resident> Residents => Set<Resident>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<BillLineItem> BillLineItems => Set<BillLineItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentProof> PaymentProofs => Set<PaymentProof>();
    public DbSet<Dispute> Disputes => Set<Dispute>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationToken> NotificationTokens => Set<NotificationToken>();
    public DbSet<BillingItem> BillingItems => Set<BillingItem>();
    public DbSet<UnitBillingRate> UnitBillingRates => Set<UnitBillingRate>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Property>()
            .HasMany(p => p.Units)
            .WithOne(u => u.Property)
            .HasForeignKey(u => u.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Unit>()
            .HasMany(u => u.Residents)
            .WithOne(r => r.Unit)
            .HasForeignKey(r => r.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unit 1—1 Account (corrected: Account links to Unit, not Resident)
        modelBuilder.Entity<Unit>()
            .HasOne(u => u.Account)
            .WithOne(a => a.Unit)
            .HasForeignKey<Account>(a => a.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Account>()
            .HasMany(a => a.Bills)
            .WithOne(b => b.Account)
            .HasForeignKey(b => b.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Account>()
            .HasMany(a => a.Payments)
            .WithOne(p => p.Account)
            .HasForeignKey(p => p.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Bill>()
            .HasMany(b => b.BillLineItems)
            .WithOne(bli => bli.Bill)
            .HasForeignKey(bli => bli.BillId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Bill>()
            .HasMany(b => b.PaymentProofs)
            .WithOne(pp => pp.Bill)
            .HasForeignKey(pp => pp.BillId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Bill>()
            .HasMany(b => b.Disputes)
            .WithOne(d => d.Bill)
            .HasForeignKey(d => d.BillId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BillLineItem>()
            .HasOne(bli => bli.BillingItem)
            .WithMany(bi => bi.BillLineItems)
            .HasForeignKey(bli => bli.BillingItemId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Resident>()
            .HasMany(r => r.Notifications)
            .WithOne(n => n.Resident)
            .HasForeignKey(n => n.ResidentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Resident>()
            .HasMany(r => r.NotificationTokens)
            .WithOne(nt => nt.Resident)
            .HasForeignKey(nt => nt.ResidentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UnitBillingRate>()
            .HasOne(ubr => ubr.Unit)
            .WithMany(u => u.UnitBillingRates)
            .HasForeignKey(ubr => ubr.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UnitBillingRate>()
            .HasOne(ubr => ubr.BillingItem)
            .WithMany(bi => bi.UnitBillingRates)
            .HasForeignKey(ubr => ubr.BillingItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AdminUser>()
            .HasMany(au => au.AuditLogs)
            .WithOne(al => al.AdminUser)
            .HasForeignKey(al => al.AdminUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Resident>()
            .HasIndex(r => r.Email)
            .IsUnique();

        modelBuilder.Entity<AdminUser>()
            .HasIndex(au => au.Email)
            .IsUnique();
    }
}
