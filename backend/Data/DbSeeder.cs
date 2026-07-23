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

    private const string DemoPassword = "Test12345";

    // Identifies demo accounts for both seeding and clearing — the single source of truth
    // for "which 5 emails are the test accounts", so SeedDemoAsync and ClearDemoAsync can't
    // drift apart on who counts as demo data.
    private static readonly (string Email, string UnitNumber, string Name)[] DemoAccounts =
    [
        ("PBtest1@propertybill.test", "A-0201", "Test User One"),
        ("PBtest2@propertybill.test", "A-0202", "Test User Two"),
        ("PBtest3@propertybill.test", "A-0203", "Test User Three"),
        ("PBtest4@propertybill.test", "A-0204", "Test User Four"),
        ("PBtest5@propertybill.test", "A-0205", "Test User Five"),
    ];

    // Wipes only the 5 demo accounts (and everything hanging off them) so SeedDemoAsync can
    // insert them fresh — never touches Alice/Benjamin or any other data. Resident->Account
    // is Restrict (not cascade, see AppDbContext), so Accounts are deleted explicitly after
    // their Residents; everything else cascades at the DB level from Unit/Resident/Bill, but
    // is still removed explicitly here (same style as ClearAsync) so this doesn't depend on
    // migration-level cascade behavior to be correct. No-op on a database that has never run
    // --seed-demo (every query below simply returns nothing).
    public static async Task ClearDemoAsync(AppDbContext context)
    {
        var demoEmails = DemoAccounts.Select(d => d.Email).ToArray();

        var residents = await context.Residents
            .Where(r => demoEmails.Contains(r.Email))
            .ToListAsync();
        if (residents.Count == 0)
        {
            return;
        }

        var residentIds = residents.Select(r => r.ResidentId).ToList();
        var accountIds = residents.Select(r => r.AccountId).ToList();
        var unitIds = residents.Select(r => r.UnitId).ToList();
        var billIds = await context.Bills
            .Where(b => unitIds.Contains(b.UnitId))
            .Select(b => b.BillId)
            .ToListAsync();

        context.Notifications.RemoveRange(context.Notifications.Where(n => residentIds.Contains(n.ResidentId)));
        context.Disputes.RemoveRange(context.Disputes.Where(d => residentIds.Contains(d.ResidentId)));
        context.Payments.RemoveRange(context.Payments.Where(p => billIds.Contains(p.BillId)));
        context.PaymentProofs.RemoveRange(context.PaymentProofs.Where(pp => residentIds.Contains(pp.ResidentId)));
        context.BillLineItems.RemoveRange(context.BillLineItems.Where(bli => billIds.Contains(bli.BillId)));
        context.Bills.RemoveRange(context.Bills.Where(b => unitIds.Contains(b.UnitId)));
        context.Residents.RemoveRange(residents);
        await context.SaveChangesAsync();

        context.Accounts.RemoveRange(context.Accounts.Where(a => accountIds.Contains(a.AccountId)));
        context.Units.RemoveRange(context.Units.Where(u => unitIds.Contains(u.UnitId)));
        await context.SaveChangesAsync();
    }

    // 5 identical demo accounts for presentation/video-recording practice — same amounts,
    // statuses, and notification mix on every account, only unit/name/email differing, so a
    // presenter can pick any one of the 5 without the demo looking different. Requires
    // SeedAsync to have already run (needs the Skyview Residence property to hang units off).
    // Always call ClearDemoAsync first (Program.cs's --seed-demo does) — this method itself
    // doesn't guard against duplicates, unlike SeedAsync/SeedExtraAsync, since "wipe and
    // recreate every time" is the whole point (resettable between practice runs).
    public static async Task SeedDemoAsync(AppDbContext context)
    {
        var property = await context.Properties.FirstOrDefaultAsync(p => p.Name == "Skyview Residence");
        if (property is null)
        {
            // Base seed (SeedAsync) hasn't run yet — nothing to hang demo units off.
            return;
        }

        foreach (var (email, unitNumber, name) in DemoAccounts)
        {
            var unit = new Unit { Property = property, UnitNumber = unitNumber, Floor = 2, Type = "2-Bedroom", IsActive = true };
            var account = new Account { CumulativeArrears = 0.00m, CreditBalance = 60.00m };
            var resident = new Resident
            {
                Unit = unit,
                Account = account,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(DemoPassword),
                Name = name,
                PhoneNumber = "010-0000000",
                NotificationPreferences = "{}",
                IsActive = true,
            };

            // Reference numbers follow the existing "SKV-{yyyy-mm}-{UnitNumber, dashes
            // stripped}" convention (see SeedAsync/SeedExtraAsync) — "A-0201" -> "A0201".
            var refSuffix = unitNumber.Replace("-", "");
            Bill MakeBill(string period, DateTime issueDate, DateTime dueDate, decimal total, decimal outstanding, string status, (string Desc, decimal Amount, string Type)[] lines)
            {
                var bill = new Bill
                {
                    Unit = unit,
                    BillingPeriod = period,
                    ReferenceNumber = $"SKV-{period}-{refSuffix}",
                    IssueDate = issueDate,
                    DueDate = dueDate,
                    TotalAmount = total,
                    OutstandingBalance = outstanding,
                    Status = status,
                };
                foreach (var (desc, amount, type) in lines)
                {
                    bill.BillLineItems.Add(new BillLineItem { Description = desc, Amount = amount, LineItemType = type });
                }
                return bill;
            }

            // --- 8 Paid bills (Aug 2025 - Mar 2026), fixed Maintenance/Sinking/Parking/
            // Insurance with a varying Water Charges line so totals differ realistically. ---
            var augBill = MakeBill("2025-08", new DateTime(2025, 8, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 0, 0, 0, DateTimeKind.Utc), 410.00m, 0.00m, "Paid",
                [("Maintenance Fee", 300.00m, "Charge"), ("Sinking Fund", 50.00m, "Charge"), ("Water Charges", 18.00m, "Charge"), ("Parking", 30.00m, "Charge"), ("Insurance", 12.00m, "Charge")]);
            var sepBill = MakeBill("2025-09", new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 15, 0, 0, 0, DateTimeKind.Utc), 414.50m, 0.00m, "Paid",
                [("Maintenance Fee", 300.00m, "Charge"), ("Sinking Fund", 50.00m, "Charge"), ("Water Charges", 22.50m, "Charge"), ("Parking", 30.00m, "Charge"), ("Insurance", 12.00m, "Charge")]);
            var octBill = MakeBill("2025-10", new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 15, 0, 0, 0, DateTimeKind.Utc), 417.00m, 0.00m, "Paid",
                [("Maintenance Fee", 300.00m, "Charge"), ("Sinking Fund", 50.00m, "Charge"), ("Water Charges", 25.00m, "Charge"), ("Parking", 30.00m, "Charge"), ("Insurance", 12.00m, "Charge")]);
            var novBill = MakeBill("2025-11", new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 12, 15, 0, 0, 0, DateTimeKind.Utc), 408.00m, 0.00m, "Paid",
                [("Maintenance Fee", 300.00m, "Charge"), ("Sinking Fund", 50.00m, "Charge"), ("Water Charges", 16.00m, "Charge"), ("Parking", 30.00m, "Charge"), ("Insurance", 12.00m, "Charge")]);
            var decBill = MakeBill("2025-12", new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), 421.00m, 0.00m, "Paid",
                [("Maintenance Fee", 300.00m, "Charge"), ("Sinking Fund", 50.00m, "Charge"), ("Water Charges", 29.00m, "Charge"), ("Parking", 30.00m, "Charge"), ("Insurance", 12.00m, "Charge")]);
            var janBill = MakeBill("2026-01", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc), 413.50m, 0.00m, "Paid",
                [("Maintenance Fee", 300.00m, "Charge"), ("Sinking Fund", 50.00m, "Charge"), ("Water Charges", 21.50m, "Charge"), ("Parking", 30.00m, "Charge"), ("Insurance", 12.00m, "Charge")]);
            var febBill = MakeBill("2026-02", new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc), 419.00m, 0.00m, "Paid",
                [("Maintenance Fee", 300.00m, "Charge"), ("Sinking Fund", 50.00m, "Charge"), ("Water Charges", 27.00m, "Charge"), ("Parking", 30.00m, "Charge"), ("Insurance", 12.00m, "Charge")]);
            var marBill = MakeBill("2026-03", new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc), 416.00m, 0.00m, "Paid",
                [("Maintenance Fee", 300.00m, "Charge"), ("Sinking Fund", 50.00m, "Charge"), ("Water Charges", 24.00m, "Charge"), ("Parking", 30.00m, "Charge"), ("Insurance", 12.00m, "Charge")]);

            // Apr 2026: Overdue, left clean (no dispute/proof) for a live dispute demo. Due
            // date set ~3 months before the date this seeder was written (2026-07-23) rather
            // than the usual issue-date-plus-45-days convention, so it reads clearly overdue
            // whenever the demo is actually presented.
            var aprBill = MakeBill("2026-04", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc), 420.50m, 420.50m, "Unpaid",
                [("Maintenance Fee", 280.00m, "Charge"), ("Sinking Fund", 56.00m, "Charge"), ("Water Charges", 32.50m, "Charge"), ("Parking", 30.00m, "Charge"), ("Insurance", 12.00m, "Charge"), ("Outstanding b/f", 0.00m, "BroughtForward"), ("Late Interest (1%)", 10.00m, "Penalty")]);

            // May 2026: left as "ProofSubmitted" via a Pending PaymentProof/Payment below.
            var mayBill = MakeBill("2026-05", new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc), 350.00m, 350.00m, "ProofSubmitted",
                [("Maintenance Fee", 300.00m, "Charge"), ("Sinking Fund", 50.00m, "Charge")]);

            // Jun 2026: Unpaid with an Open dispute (Disputed badge). Due date pushed out to
            // Aug so this stays "Unpaid" (not "Overdue") for a while regardless of exactly
            // when --seed-demo is run/demoed, same reasoning as SeedExtraAsync's julBill.
            var junBill = MakeBill("2026-06", new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 15, 0, 0, 0, DateTimeKind.Utc), 395.00m, 395.00m, "Unpaid",
                [("Maintenance Fee", 300.00m, "Charge"), ("Sinking Fund", 50.00m, "Charge"), ("Water Charges", 20.00m, "Charge"), ("Parking", 25.00m, "Charge")]);

            // Jul 2026: Unpaid, clean, due ~3 weeks out — left for a live payment-proof demo.
            var julBill = MakeBill("2026-07", new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 13, 0, 0, 0, DateTimeKind.Utc), 365.00m, 365.00m, "Unpaid",
                [("Maintenance Fee", 300.00m, "Charge"), ("Sinking Fund", 50.00m, "Charge"), ("Water Charges", 15.00m, "Charge")]);

            var payments = new List<Payment>
            {
                new() { Bill = augBill, Amount = 410.00m, PaymentDate = new DateTime(2025, 8, 20, 0, 0, 0, DateTimeKind.Utc), Channel = "Bank Transfer", Status = "Confirmed" },
                new() { Bill = sepBill, Amount = 414.50m, PaymentDate = new DateTime(2025, 9, 8, 0, 0, 0, DateTimeKind.Utc), Channel = "Online Banking", Status = "Confirmed" },
                new() { Bill = octBill, Amount = 417.00m, PaymentDate = new DateTime(2025, 10, 18, 0, 0, 0, DateTimeKind.Utc), Channel = "Credit Card", Status = "Confirmed" },
                new() { Bill = novBill, Amount = 408.00m, PaymentDate = new DateTime(2025, 11, 22, 0, 0, 0, DateTimeKind.Utc), Channel = "e-Wallet", Status = "Confirmed" },
                new() { Bill = decBill, Amount = 421.00m, PaymentDate = new DateTime(2025, 12, 15, 0, 0, 0, DateTimeKind.Utc), Channel = "Bank Transfer", Status = "Confirmed" },
                new() { Bill = janBill, Amount = 413.50m, PaymentDate = new DateTime(2026, 1, 8, 0, 0, 0, DateTimeKind.Utc), Channel = "Online Banking", Status = "Confirmed" },
                new() { Bill = febBill, Amount = 419.00m, PaymentDate = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc), Channel = "Credit Card", Status = "Confirmed" },
                new() { Bill = marBill, Amount = 416.00m, PaymentDate = new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc), Channel = "Bank Transfer", Status = "Confirmed" },
            };

            // Placeholder URL, not a real upload — per the task, no file storage round-trip
            // needed for this to demo "submission history" correctly.
            var mayProof = new PaymentProof
            {
                Resident = resident,
                FileUrl = "https://placeholder.propertybill.test/demo-payment-proof.png",
                FileType = "image/png",
                FileSize = 245760,
                Status = "Pending",
                SubmittedAt = new DateTime(2026, 5, 8, 11, 0, 0, DateTimeKind.Utc),
            };
            // Mirrors PaymentProofRepository.CreateAsync's real tagging shape exactly (Channel
            // "Proof Upload", Status "Pending", Amount = the tagged bill's OutstandingBalance)
            // so the Pay tab's submission history looks identical to a real upload.
            var mayProofPayment = new Payment
            {
                Bill = mayBill,
                PaymentProof = mayProof,
                Amount = mayBill.OutstandingBalance,
                PaymentDate = mayProof.SubmittedAt,
                Channel = "Proof Upload",
                Status = "Pending",
            };

            var marResolvedDispute = new Dispute
            {
                Bill = marBill,
                Resident = resident,
                Reason = "The parking charge on this bill doesn't match what I was told when I moved in — please review.",
                Status = "Resolved",
                SubmittedAt = new DateTime(2026, 3, 20, 9, 0, 0, DateTimeKind.Utc),
                ResolvedAt = new DateTime(2026, 3, 27, 14, 0, 0, DateTimeKind.Utc),
                AdminResponse = "We've reviewed your account and confirmed the parking charge is correct per your unit's assigned bay. Thank you for reaching out.",
            };
            var junOpenDispute = new Dispute
            {
                Bill = junBill,
                Resident = resident,
                Reason = "The water charges for this month seem unusually high compared to my previous bills — please check the meter reading.",
                Status = "Open",
                SubmittedAt = new DateTime(2026, 6, 10, 11, 0, 0, DateTimeKind.Utc),
            };

            // 6 notifications, 3 read / 3 unread, mixing all four UC-107 types. Title/body
            // wording follows Figure 4.14 exactly ("{Type label} · {Month Year}", and
            // BillIssued's body carrying the real total) rather than the generic
            // DevController canned copy, since these appear in a real Alerts list, not a
            // one-off dev test.
            var notifications = new List<Notification>
            {
                new() { Resident = resident, Type = "BillIssued", Title = "New bill issued · August 2025", Body = "Total due RM 410.00. Tap to view details.", IsRead = true, SentAt = new DateTime(2025, 8, 1, 9, 0, 0, DateTimeKind.Utc) },
                new() { Resident = resident, Type = "PaymentConfirmed", Title = "Payment confirmed · September 2025", Body = "Your payment has been confirmed. Thank you!", IsRead = true, SentAt = new DateTime(2025, 9, 8, 10, 0, 0, DateTimeKind.Utc) },
                new() { Resident = resident, Type = "DueReminder", Title = "Due date approaching · March 2026", Body = "Your bill is due in 3 days.", IsRead = true, SentAt = new DateTime(2026, 4, 12, 9, 0, 0, DateTimeKind.Utc) },
                new() { Resident = resident, Type = "PaymentConfirmed", Title = "Payment confirmed · January 2026", Body = "Your payment has been confirmed. Thank you!", IsRead = false, SentAt = new DateTime(2026, 1, 8, 10, 0, 0, DateTimeKind.Utc) },
                new() { Resident = resident, Type = "BillOverdue", Title = "Bill overdue · April 2026", Body = "Please make payment as soon as possible.", IsRead = false, SentAt = new DateTime(2026, 5, 16, 8, 0, 0, DateTimeKind.Utc) },
                new() { Resident = resident, Type = "BillIssued", Title = "New bill issued · June 2026", Body = "Total due RM 395.00. Tap to view details.", IsRead = false, SentAt = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc) },
            };

            context.Residents.Add(resident);
            context.Bills.AddRange(augBill, sepBill, octBill, novBill, decBill, janBill, febBill, marBill, aprBill, mayBill, junBill, julBill);
            context.Payments.AddRange(payments);
            context.PaymentProofs.Add(mayProof);
            context.Payments.Add(mayProofPayment);
            context.Disputes.AddRange(marResolvedDispute, junOpenDispute);
            context.Notifications.AddRange(notifications);

            await context.SaveChangesAsync();

            // Same two-phase save as SeedExtraAsync: DeepLinks need each bill's DB-generated
            // BillId, which only exists after the save above.
            notifications[0].DeepLink = $"/(tabs)/bills/{augBill.BillId}";
            notifications[1].DeepLink = $"/(tabs)/bills/{sepBill.BillId}";
            notifications[2].DeepLink = $"/(tabs)/bills/{marBill.BillId}";
            notifications[3].DeepLink = $"/(tabs)/bills/{janBill.BillId}";
            notifications[4].DeepLink = $"/(tabs)/bills/{aprBill.BillId}";
            notifications[5].DeepLink = $"/(tabs)/bills/{junBill.BillId}";
            await context.SaveChangesAsync();
        }
    }
}
