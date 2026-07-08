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
        // Explicit keys for entities whose PK doesn't follow the {EntityName}Id convention.
        modelBuilder.Entity<BillLineItem>().HasKey(bli => bli.LineItemId);
        modelBuilder.Entity<PaymentProof>().HasKey(pp => pp.ProofId);
        modelBuilder.Entity<NotificationToken>().HasKey(nt => nt.TokenId);

        modelBuilder.Entity<Property>()
            .HasMany(p => p.Units)
            .WithOne(u => u.Property)
            .HasForeignKey(u => u.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Property>()
            .HasMany(p => p.BillingItems)
            .WithOne(bi => bi.Property)
            .HasForeignKey(bi => bi.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Unit>()
            .HasMany(u => u.Residents)
            .WithOne(r => r.Unit)
            .HasForeignKey(r => r.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Unit>()
            .HasMany(u => u.Bills)
            .WithOne(b => b.Unit)
            .HasForeignKey(b => b.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Unit>()
            .HasMany(u => u.UnitBillingRates)
            .WithOne(ubr => ubr.Unit)
            .HasForeignKey(ubr => ubr.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Resident 1—1 Account. Account is the principal side (no FK of its own); Resident
        // holds the FK. Restrict rather than Cascade so an Account can't vanish out from
        // under a Resident row as a side effect of some other delete.
        modelBuilder.Entity<Resident>()
            .HasOne(r => r.Account)
            .WithOne(a => a.Resident)
            .HasForeignKey<Resident>(r => r.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Resident>()
            .HasMany(r => r.PaymentProofs)
            .WithOne(pp => pp.Resident)
            .HasForeignKey(pp => pp.ResidentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Resident>()
            .HasMany(r => r.Disputes)
            .WithOne(d => d.Resident)
            .HasForeignKey(d => d.ResidentId)
            .OnDelete(DeleteBehavior.Cascade);

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

        modelBuilder.Entity<Bill>()
            .HasMany(b => b.BillLineItems)
            .WithOne(bli => bli.Bill)
            .HasForeignKey(bli => bli.BillId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Bill>()
            .HasMany(b => b.Payments)
            .WithOne(p => p.Bill)
            .HasForeignKey(p => p.BillId)
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

        // Payment → PaymentProof is a nullable FK on the Payment side, set to null (not
        // cascaded) on delete: removing a proof shouldn't remove the payment record it
        // was attached to, and this keeps Payment/PaymentProof from forming a hard
        // mutual dependency in the model.
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.PaymentProof)
            .WithMany(pp => pp.Payments)
            .HasForeignKey(p => p.ProofId)
            .OnDelete(DeleteBehavior.SetNull);

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

        modelBuilder.Entity<Resident>()
            .HasIndex(r => r.AccountId)
            .IsUnique();

        modelBuilder.Entity<Bill>()
            .HasIndex(b => b.ReferenceNumber)
            .IsUnique();

        modelBuilder.Entity<AdminUser>()
            .HasIndex(au => au.Email)
            .IsUnique();

        modelBuilder.Entity<AdminUser>()
            .HasIndex(au => au.Username)
            .IsUnique();
    }
}
