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
            ContactEmail = "management@skyview.my",
            ContactPhone = "+60-3-1234-5678",
        };

        var unitA = new Unit { UnitNumber = "A-01-01", Floor = 1, Type = "3-Bedroom", IsActive = true, Property = property };
        var unitB = new Unit { UnitNumber = "A-01-02", Floor = 1, Type = "2-Bedroom", IsActive = true, Property = property };

        var accountA = new Account { CumulativeArrears = 350.00m, CreditBalance = 0m };
        var accountB = new Account { CumulativeArrears = 0m, CreditBalance = 0m };

        var residentA = new Resident
        {
            Unit = unitA,
            Account = accountA,
            Email = "alice.tan@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword),
            Name = "Alice Tan",
            PhoneNumber = "012-3456789",
            NotificationPreferences = "{}",
            IsActive = true,
        };

        var residentB = new Resident
        {
            Unit = unitB,
            Account = accountB,
            Email = "benjamin.lee@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword),
            Name = "Benjamin Lee",
            PhoneNumber = "012-9876543",
            NotificationPreferences = "{}",
            IsActive = true,
        };

        var juneBill = new Bill
        {
            Unit = unitA,
            BillingPeriod = "2026-06",
            ReferenceNumber = "SKV-2026-06-A0101",
            IssueDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            TotalAmount = 350.00m,
            OutstandingBalance = 350.00m,
            Status = "Pending",
            DueDate = new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc),
        };
        juneBill.BillLineItems.Add(new BillLineItem { Description = "Maintenance Fee", Amount = 300.00m, LineItemType = "Charge" });
        juneBill.BillLineItems.Add(new BillLineItem { Description = "Sinking Fund", Amount = 50.00m, LineItemType = "Charge" });

        var mayBill = new Bill
        {
            Unit = unitA,
            BillingPeriod = "2026-05",
            ReferenceNumber = "SKV-2026-05-A0101",
            IssueDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            TotalAmount = 350.00m,
            OutstandingBalance = 0.00m,
            Status = "Paid",
            DueDate = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc),
        };
        mayBill.BillLineItems.Add(new BillLineItem { Description = "Maintenance Fee", Amount = 300.00m, LineItemType = "Charge" });
        mayBill.BillLineItems.Add(new BillLineItem { Description = "Sinking Fund", Amount = 50.00m, LineItemType = "Charge" });

        context.Properties.Add(property);
        context.Residents.AddRange(residentA, residentB);
        context.Bills.AddRange(juneBill, mayBill);

        await context.SaveChangesAsync();
    }
}
