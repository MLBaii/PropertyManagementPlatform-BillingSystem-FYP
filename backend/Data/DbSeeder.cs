using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Models;

namespace PropertyBill.Api.Data;

// Development-only test data. Idempotent: skips if a Property already exists.
public static class DbSeeder
{
    private const string TestPassword = "Test1234";

    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Properties.AnyAsync())
        {
            return;
        }

        var property = new Property
        {
            Name = "Skyview Residence",
            Address = "123 Jalan Skyview, Kuala Lumpur",
        };

        var unitA = new Unit { UnitNumber = "A-01-01", Property = property };
        var unitB = new Unit { UnitNumber = "A-01-02", Property = property };

        var accountA = new Account { Unit = unitA, Balance = 350.00m };
        var accountB = new Account { Unit = unitB, Balance = 0m };

        var residentA = new Resident
        {
            Unit = unitA,
            FullName = "Alice Tan",
            Email = "alice.tan@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword),
            Phone = "012-3456789",
            IsActive = true,
        };

        var residentB = new Resident
        {
            Unit = unitB,
            FullName = "Benjamin Lee",
            Email = "benjamin.lee@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword),
            Phone = "012-9876543",
            IsActive = true,
        };

        var juneBill = new Bill
        {
            Account = accountA,
            BillingPeriodStart = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            BillingPeriodEnd = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc),
            DueDate = new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc),
            TotalAmount = 350.00m,
            Status = "Pending",
        };
        juneBill.BillLineItems.Add(new BillLineItem { Description = "Maintenance Fee", Amount = 300.00m });
        juneBill.BillLineItems.Add(new BillLineItem { Description = "Sinking Fund", Amount = 50.00m });

        var mayBill = new Bill
        {
            Account = accountA,
            BillingPeriodStart = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            BillingPeriodEnd = new DateTime(2026, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            DueDate = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc),
            TotalAmount = 350.00m,
            Status = "Paid",
        };
        mayBill.BillLineItems.Add(new BillLineItem { Description = "Maintenance Fee", Amount = 300.00m });
        mayBill.BillLineItems.Add(new BillLineItem { Description = "Sinking Fund", Amount = 50.00m });

        context.Properties.Add(property);
        context.Residents.AddRange(residentA, residentB);
        context.Bills.AddRange(juneBill, mayBill);

        await context.SaveChangesAsync();
    }
}
