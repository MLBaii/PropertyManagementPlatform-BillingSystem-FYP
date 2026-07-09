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

        // Raw Bill.Status only ever holds "Unpaid" or "Paid" — "Overdue" (and future
        // "ProofSubmitted") are derived in BillService from the due date / payment
        // state, since nothing here runs a scheduled job to flip a stored status over.
        var juneBill = new Bill
        {
            Unit = unitA,
            BillingPeriod = "2026-06",
            ReferenceNumber = "SKV-2026-06-A0101",
            IssueDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            TotalAmount = 350.00m,
            OutstandingBalance = 350.00m,
            Status = "Unpaid",
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

        // Backs mayBill's "Paid" status with an actual Payment row — needed for the
        // Dashboard's Total Paid figure and Recent Activity feed (UC-104) to have real
        // data, since nothing previously created one despite the bill being marked Paid.
        var mayPayment = new Payment
        {
            Bill = mayBill,
            Amount = 350.00m,
            PaymentDate = new DateTime(2026, 6, 5, 0, 0, 0, DateTimeKind.Utc),
            Channel = "Bank Transfer",
            Status = "Confirmed",
        };

        // Overdue example with a richer line-item breakdown (brought-forward + late
        // interest), mirroring Figure 4.11 of the mockups so the Bill Detail screen
        // has real data to match against.
        var aprilBill = new Bill
        {
            Unit = unitA,
            BillingPeriod = "2026-04",
            ReferenceNumber = "SKV-2026-04-A0101",
            IssueDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            TotalAmount = 420.50m,
            OutstandingBalance = 420.50m,
            Status = "Unpaid",
            DueDate = new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc),
        };
        aprilBill.BillLineItems.Add(new BillLineItem { Description = "Maintenance Fee", Amount = 280.00m, LineItemType = "Charge" });
        aprilBill.BillLineItems.Add(new BillLineItem { Description = "Sinking Fund", Amount = 56.00m, LineItemType = "Charge" });
        aprilBill.BillLineItems.Add(new BillLineItem { Description = "Water Charges", Amount = 32.50m, LineItemType = "Charge" });
        aprilBill.BillLineItems.Add(new BillLineItem { Description = "Parking", Amount = 30.00m, LineItemType = "Charge" });
        aprilBill.BillLineItems.Add(new BillLineItem { Description = "Insurance", Amount = 12.00m, LineItemType = "Charge" });
        aprilBill.BillLineItems.Add(new BillLineItem { Description = "Outstanding b/f", Amount = 0.00m, LineItemType = "BroughtForward" });
        aprilBill.BillLineItems.Add(new BillLineItem { Description = "Late Interest (1%)", Amount = 10.00m, LineItemType = "Penalty" });

        context.Properties.Add(property);
        context.Residents.AddRange(residentA, residentB);
        context.Bills.AddRange(juneBill, mayBill, aprilBill);
        context.Payments.Add(mayPayment);

        await context.SaveChangesAsync();
    }

    // Drops all seeded rows (FK-safe order) so SeedAsync can insert fresh data,
    // e.g. after changing what gets seeded. Dev-only — never call this outside --reseed.
    public static async Task ClearAsync(AppDbContext context)
    {
        context.Payments.RemoveRange(context.Payments);
        context.BillLineItems.RemoveRange(context.BillLineItems);
        context.Bills.RemoveRange(context.Bills);
        context.Residents.RemoveRange(context.Residents);
        context.Accounts.RemoveRange(context.Accounts);
        context.Units.RemoveRange(context.Units);
        context.Properties.RemoveRange(context.Properties);
        await context.SaveChangesAsync();
    }
}
