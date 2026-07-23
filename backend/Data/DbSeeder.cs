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
            await EnsureAdminUserAsync(context);
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
        await EnsureAdminUserAsync(context);
    }

    private static async Task EnsureAdminUserAsync(AppDbContext context)
    {
        if (await context.AdminUsers.AnyAsync(user => user.Username == "admin")) return;
        context.AdminUsers.Add(new AdminUser { Username = "admin", Email = "admin@skyview.my", PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword), Role = "Admin" });
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

    // Purely additive test data for Alice Tan, layered on top of whatever SeedAsync already
    // put there — never touches existing rows, and never calls ClearAsync. Exists so list
    // views / filters / edge cases (Bills filters, Receipts, Alerts unread badge, Dispute
    // history + admin response) have enough varied data to exercise, without the admin portal
    // that would normally produce it. Idempotent: guarded on a marker ReferenceNumber so
    // running --seed-extra twice doesn't duplicate rows.
    public static async Task SeedExtraAsync(AppDbContext context)
    {
        const string markerReferenceNumber = "SKV-2026-01-A0101";
        if (await context.Bills.AnyAsync(b => b.ReferenceNumber == markerReferenceNumber))
        {
            return;
        }

        var alice = await context.Residents
            .Include(r => r.Unit)
            .FirstOrDefaultAsync(r => r.Email == "alice.tan@example.com");
        if (alice is null)
        {
            // Base seed (SeedAsync) hasn't run yet — nothing to layer extra data on top of.
            return;
        }

        var unitA = alice.Unit;

        // Existing bills from SeedAsync, needed here only so BillOverdue notifications below
        // can carry a real deep link — read-only, never modified.
        var existingAprilBill = await context.Bills.FirstAsync(b => b.ReferenceNumber == "SKV-2026-04-A0101");
        var existingJuneBill = await context.Bills.FirstAsync(b => b.ReferenceNumber == "SKV-2026-06-A0101");

        // --- Bills: 2 more Paid, 2 more Unpaid (due date in the future), 1 more Overdue ---
        var janBill = new Bill
        {
            Unit = unitA,
            BillingPeriod = "2026-01",
            ReferenceNumber = markerReferenceNumber,
            IssueDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TotalAmount = 350.00m,
            OutstandingBalance = 0.00m,
            Status = "Paid",
            DueDate = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc),
        };
        janBill.BillLineItems.Add(new BillLineItem { Description = "Maintenance Fee", Amount = 300.00m, LineItemType = "Charge" });
        janBill.BillLineItems.Add(new BillLineItem { Description = "Sinking Fund", Amount = 50.00m, LineItemType = "Charge" });

        var febBill = new Bill
        {
            Unit = unitA,
            BillingPeriod = "2026-02",
            ReferenceNumber = "SKV-2026-02-A0101",
            IssueDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            TotalAmount = 420.00m,
            OutstandingBalance = 0.00m,
            Status = "Paid",
            DueDate = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc),
        };
        febBill.BillLineItems.Add(new BillLineItem { Description = "Maintenance Fee", Amount = 300.00m, LineItemType = "Charge" });
        febBill.BillLineItems.Add(new BillLineItem { Description = "Sinking Fund", Amount = 50.00m, LineItemType = "Charge" });
        febBill.BillLineItems.Add(new BillLineItem { Description = "Water Charges", Amount = 28.00m, LineItemType = "Charge" });
        febBill.BillLineItems.Add(new BillLineItem { Description = "Parking", Amount = 30.00m, LineItemType = "Charge" });
        febBill.BillLineItems.Add(new BillLineItem { Description = "Insurance", Amount = 12.00m, LineItemType = "Charge" });

        var marBill = new Bill
        {
            Unit = unitA,
            BillingPeriod = "2026-03",
            ReferenceNumber = "SKV-2026-03-A0101",
            IssueDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            TotalAmount = 395.00m,
            OutstandingBalance = 395.00m,
            Status = "Unpaid",
            // Past due date so BillService.ComputeEffectiveStatus derives "Overdue".
            DueDate = new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc),
        };
        marBill.BillLineItems.Add(new BillLineItem { Description = "Maintenance Fee", Amount = 300.00m, LineItemType = "Charge" });
        marBill.BillLineItems.Add(new BillLineItem { Description = "Sinking Fund", Amount = 50.00m, LineItemType = "Charge" });
        marBill.BillLineItems.Add(new BillLineItem { Description = "Water Charges", Amount = 20.00m, LineItemType = "Charge" });
        marBill.BillLineItems.Add(new BillLineItem { Description = "Parking", Amount = 25.00m, LineItemType = "Charge" });

        var julBill = new Bill
        {
            Unit = unitA,
            BillingPeriod = "2026-07",
            ReferenceNumber = "SKV-2026-07-A0101",
            IssueDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
            TotalAmount = 365.00m,
            OutstandingBalance = 365.00m,
            Status = "Unpaid",
            // Future due date so this stays "Unpaid" (not "Overdue") regardless of when
            // --seed-extra is actually run, as long as it's before 2026-08-15.
            DueDate = new DateTime(2026, 8, 15, 0, 0, 0, DateTimeKind.Utc),
        };
        julBill.BillLineItems.Add(new BillLineItem { Description = "Maintenance Fee", Amount = 300.00m, LineItemType = "Charge" });
        julBill.BillLineItems.Add(new BillLineItem { Description = "Sinking Fund", Amount = 50.00m, LineItemType = "Charge" });
        julBill.BillLineItems.Add(new BillLineItem { Description = "Water Charges", Amount = 15.00m, LineItemType = "Charge" });

        var augBill = new Bill
        {
            Unit = unitA,
            BillingPeriod = "2026-08",
            ReferenceNumber = "SKV-2026-08-A0101",
            // Issued a couple weeks ahead of the period, same as a property manager billing
            // in advance — still a future due date either way.
            IssueDate = new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc),
            TotalAmount = 422.00m,
            OutstandingBalance = 422.00m,
            Status = "Unpaid",
            DueDate = new DateTime(2026, 8, 31, 0, 0, 0, DateTimeKind.Utc),
        };
        augBill.BillLineItems.Add(new BillLineItem { Description = "Maintenance Fee", Amount = 300.00m, LineItemType = "Charge" });
        augBill.BillLineItems.Add(new BillLineItem { Description = "Sinking Fund", Amount = 50.00m, LineItemType = "Charge" });
        augBill.BillLineItems.Add(new BillLineItem { Description = "Water Charges", Amount = 30.00m, LineItemType = "Charge" });
        augBill.BillLineItems.Add(new BillLineItem { Description = "Parking", Amount = 30.00m, LineItemType = "Charge" });
        augBill.BillLineItems.Add(new BillLineItem { Description = "Insurance", Amount = 12.00m, LineItemType = "Charge" });

        // --- Payments: 2 more Confirmed, each backing one of the new Paid bills ---
        var janPayment = new Payment
        {
            Bill = janBill,
            Amount = 350.00m,
            PaymentDate = new DateTime(2026, 1, 25, 0, 0, 0, DateTimeKind.Utc),
            Channel = "Online Banking",
            Status = "Confirmed",
        };
        var febPayment = new Payment
        {
            Bill = febBill,
            Amount = 420.00m,
            PaymentDate = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc),
            Channel = "Credit Card",
            Status = "Confirmed",
        };

        // --- Notifications: 3 of each type, mixed read/unread, spread across dates so the
        // Alerts list has a real chronological order and the unread badge shows > 1. ---
        var notifications = new List<Notification>
        {
            new()
            {
                Resident = alice, Type = "BillIssued", Title = "New bill issued",
                Body = "A new bill has been issued for your unit. Tap to view details.",
                IsRead = true,
                SentAt = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Resident = alice, Type = "BillIssued", Title = "New bill issued",
                Body = "A new bill has been issued for your unit. Tap to view details.",
                IsRead = true,
                SentAt = new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Resident = alice, Type = "BillIssued", Title = "New bill issued",
                Body = "A new bill has been issued for your unit. Tap to view details.",
                IsRead = false,
                SentAt = new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Resident = alice, Type = "BillOverdue", Title = "Bill overdue",
                Body = "Please make payment as soon as possible.",
                IsRead = true,
                SentAt = new DateTime(2026, 4, 16, 8, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Resident = alice, Type = "BillOverdue", Title = "Bill overdue",
                Body = "Please make payment as soon as possible.",
                IsRead = true,
                SentAt = new DateTime(2026, 5, 16, 8, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Resident = alice, Type = "BillOverdue", Title = "Bill overdue",
                Body = "Please make payment as soon as possible.",
                IsRead = false,
                SentAt = new DateTime(2026, 7, 16, 8, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Resident = alice, Type = "PaymentConfirmed", Title = "Payment confirmed",
                Body = "Your payment has been confirmed. Thank you!",
                IsRead = true,
                SentAt = new DateTime(2026, 1, 25, 10, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Resident = alice, Type = "PaymentConfirmed", Title = "Payment confirmed",
                Body = "Your payment has been confirmed. Thank you!",
                IsRead = true,
                SentAt = new DateTime(2026, 2, 20, 10, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Resident = alice, Type = "PaymentConfirmed", Title = "Payment confirmed",
                Body = "Your payment has been confirmed. Thank you!",
                IsRead = false,
                SentAt = new DateTime(2026, 6, 5, 10, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Resident = alice, Type = "DueReminder", Title = "Due date approaching",
                Body = "Your bill is due soon.",
                IsRead = true,
                SentAt = new DateTime(2026, 2, 8, 9, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Resident = alice, Type = "DueReminder", Title = "Due date approaching",
                Body = "Your bill is due soon.",
                IsRead = true,
                SentAt = new DateTime(2026, 3, 8, 9, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Resident = alice, Type = "DueReminder", Title = "Due date approaching",
                Body = "Your bill is due soon.",
                IsRead = false,
                SentAt = new DateTime(2026, 4, 8, 9, 0, 0, DateTimeKind.Utc),
            },
        };

        // --- Disputes: Open, Under Review, and Resolved (with an admin response) across
        // three different bills — since nothing in this project can set AdminResponse
        // through the app itself (no admin portal), this is the only way to see that UI. ---
        var openDispute = new Dispute
        {
            Bill = julBill,
            Resident = alice,
            Reason = "The maintenance fee amount is different from what was stated in my unit purchase agreement — please clarify the breakdown.",
            Status = "Open",
            SubmittedAt = new DateTime(2026, 7, 10, 11, 0, 0, DateTimeKind.Utc),
        };
        var underReviewDispute = new Dispute
        {
            Bill = marBill,
            Resident = alice,
            Reason = "The water charges for this period seem higher than usual — please double-check the meter reading used for this billing cycle.",
            Status = "UnderReview",
            SubmittedAt = new DateTime(2026, 4, 20, 10, 0, 0, DateTimeKind.Utc),
        };
        var resolvedDispute = new Dispute
        {
            Bill = febBill,
            Resident = alice,
            Reason = "I was charged for parking but I do not have a parking space assigned to my unit — please review this line item.",
            Status = "Resolved",
            SubmittedAt = new DateTime(2026, 2, 25, 9, 0, 0, DateTimeKind.Utc),
            ResolvedAt = new DateTime(2026, 3, 2, 14, 0, 0, DateTimeKind.Utc),
            AdminResponse = "We've reviewed your account and confirmed parking bay P-12 is assigned to unit A-01-01 as of Jan 2026. The charge is correct and will remain on your bill. Thank you for reaching out.",
        };

        context.Bills.AddRange(janBill, febBill, marBill, julBill, augBill);
        context.Payments.AddRange(janPayment, febPayment);
        context.Notifications.AddRange(notifications);
        context.Disputes.AddRange(openDispute, underReviewDispute, resolvedDispute);

        await context.SaveChangesAsync();

        // The new bills only get a real (DB-generated) BillId after the save above, so their
        // notifications' DeepLinks are filled in as a second pass rather than at construction.
        notifications[0].DeepLink = $"/(tabs)/bills/{janBill.BillId}";
        notifications[1].DeepLink = $"/(tabs)/bills/{marBill.BillId}";
        notifications[2].DeepLink = $"/(tabs)/bills/{julBill.BillId}";
        notifications[3].DeepLink = $"/(tabs)/bills/{marBill.BillId}";
        notifications[4].DeepLink = $"/(tabs)/bills/{existingAprilBill.BillId}";
        notifications[5].DeepLink = $"/(tabs)/bills/{existingJuneBill.BillId}";
        await context.SaveChangesAsync();
    }
}
