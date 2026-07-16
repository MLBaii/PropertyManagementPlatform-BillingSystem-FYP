# API Contract (Shared)

> Backend: ASP.NET Core Web API (unchanged). Database: PostgreSQL hosted on Supabase, accessed via EF Core + Npgsql.

> Endpoints and JSON shapes are agreed jointly between Module A and Module B.
> This is the single source of truth — update here before changing either side.
> All resident endpoints are JWT-authenticated and scoped to the resident's unit.

## Endpoint groups (from §4.2.3)

| Endpoint Group | Base URL | Methods | Description |
|----------------|----------|---------|-------------|
| Authentication | `/api/auth/resident` | POST, GET | Login, token refresh, logout, password reset |
| Profile | `/api/residents/profile` | GET, PUT | View/update profile, notification prefs, password |
| Bills | `/api/residents/bills` | GET | Bill list + itemised bill detail, unit-scoped |
| Account | `/api/residents/dashboard` | GET | Dashboard data, running balance, transaction history — implemented as `/dashboard` rather than `/account` (see below) |
| Payment Proof | `/api/residents/payment-proofs` | POST, GET | Upload proof, tag to bills, retrieve status |
| Disputes | `/api/residents/disputes` | POST, GET | Submit dispute, history, admin response |
| Notifications | `/api/residents/notifications` | POST, GET, PUT | Register push token, history, mark read |
| Receipts | `/api/residents/receipts` | GET | List + detail (`/{paymentId}`) — digital receipt data for confirmed payments |

## JSON shapes

### Profile (UC-102) — implemented

All four endpoints are `[Authorize]`, scoped to the JWT's `ResidentId` claim.

#### `GET /api/residents/profile`
Response `200`:
```json
{
  "name": "Alice Tan",
  "email": "alice.tan@example.com",
  "phoneNumber": "012-3456789",
  "notificationPreferences": { "pushEnabled": true, "emailEnabled": true, "billDueReminders": true },
  "unitNumber": "A-01-01",
  "floor": 1,
  "propertyName": "Skyview Residence"
}
```
`unitNumber`/`floor`/`propertyName` are read-only — not accepted by `PUT /api/residents/profile`.

#### `PUT /api/residents/profile`
Request:
```json
{ "name": "Alice Tan", "phoneNumber": "012-3456789", "email": "alice.tan@example.com" }
```
`name` and `email` required, `email` must be a valid address. Response `200`:
```json
{ "profile": { "...": "ProfileDto, same shape as GET" }, "message": "Profile updated successfully!" }
```
`409` if `email` is already used by another resident. `404` if the resident no longer exists.

#### `PUT /api/residents/profile/password`
Request:
```json
{ "currentPassword": "Test1234", "newPassword": "NewPass1234" }
```
`newPassword` must be at least 8 characters. Response `200`:
```json
{ "message": "Your password has been changed successfully." }
```
`401` if `currentPassword` doesn't match the stored hash.

#### `PUT /api/residents/profile/notifications`
Request/response body is a `NotificationPreferencesDto`:
```json
{ "pushEnabled": true, "emailEnabled": false, "billDueReminders": true }
```
Response `200` wraps it: `{ "notificationPreferences": { ... }, "message": "Notification preferences updated." }`.

### Bills (UC-103) — implemented

Both endpoints are `[Authorize]`, scoped to the JWT's `UnitId` claim.

`status` in the response is **computed**, not the raw stored value — see `BillService.ComputeEffectiveStatus`.
Nothing in this project runs a scheduled job to flip a bill from unpaid to overdue, so "Overdue" is derived
from `dueDate` vs. the current date every time the endpoint is called, rather than being a value anyone writes.
Raw `Bill.Status` in the database holds `"Unpaid"`, `"Paid"`, or (since UC-106) `"ProofSubmitted"` — the last
one set directly by `PaymentProofRepository.CreateAsync` when a resident tags a bill to an uploaded proof,
rather than derived like "Overdue" is.

#### `GET /api/residents/bills?status={status}`
`status` query param is optional, case-insensitive, one of `Unpaid` | `Overdue` | `Paid` | `ProofSubmitted`
— filters the list to bills whose *computed* status matches. Response `200`:
```json
[
  {
    "billId": 3,
    "referenceNumber": "SKV-2026-04-A0101",
    "billingPeriod": "2026-04",
    "issueDate": "2026-04-01T00:00:00Z",
    "dueDate": "2026-05-15T00:00:00Z",
    "totalAmount": 420.50,
    "outstandingBalance": 420.50,
    "status": "Overdue",
    "daysUntilDue": -54
  }
]
```
`daysUntilDue` is positive when the due date is in the future, negative when overdue by that many days.

#### `GET /api/residents/bills/{billId}`
Response `200` — same fields as the list, plus the itemised line items and (since UC-105) the unit/property
fields the PDF masthead needs:
```json
{
  "billId": 3,
  "referenceNumber": "SKV-2026-04-A0101",
  "billingPeriod": "2026-04",
  "issueDate": "2026-04-01T00:00:00Z",
  "dueDate": "2026-05-15T00:00:00Z",
  "totalAmount": 420.50,
  "outstandingBalance": 420.50,
  "status": "Overdue",
  "daysUntilDue": -54,
  "lineItems": [
    { "lineItemId": 1, "description": "Maintenance Fee", "amount": 280.00, "lineItemType": "Charge" },
    { "lineItemId": 2, "description": "Sinking Fund", "amount": 56.00, "lineItemType": "Charge" },
    { "lineItemId": 3, "description": "Water Charges", "amount": 32.50, "lineItemType": "Charge" },
    { "lineItemId": 4, "description": "Parking", "amount": 30.00, "lineItemType": "Charge" },
    { "lineItemId": 5, "description": "Insurance", "amount": 12.00, "lineItemType": "Charge" },
    { "lineItemId": 6, "description": "Outstanding b/f", "amount": 0.00, "lineItemType": "BroughtForward" },
    { "lineItemId": 7, "description": "Late Interest (1%)", "amount": 10.00, "lineItemType": "Penalty" }
  ],
  "unitNumber": "A-01-01",
  "propertyName": "Skyview Residence"
}
```
`404` if the bill doesn't exist **or** belongs to a different unit — deliberately not distinguished, to avoid
confirming a `billId` belongs to someone else.

### PDF Bill Download (UC-105) — implemented, no backend endpoint

Per §2.4.6, PDF generation is on-device (`expo-print`), not server-rendered, so there is no new API
surface for this feature — the Bill Detail screen builds the PDF from the `GET /api/residents/bills/{billId}`
response it already has (plus the resident's name, already in `AuthContext` from login). The only backend
change was adding `unitNumber`/`propertyName` to that existing response (above), since the PDF masthead
needed unit info the Bill Detail screen didn't previously fetch.

### Dashboard (UC-104) — implemented

`[Authorize]`, scoped to the JWT's `ResidentId` **and** `UnitId` claims. Route is `/api/residents/dashboard`
(the endpoint-group table above originally sketched `/api/residents/account`; renamed to `/dashboard` to
match what this screen actually shows — an account-scoped summary is still exactly what's returned).

#### `GET /api/residents/dashboard`
Response `200`:
```json
{
  "unitNumber": "A-01-01",
  "propertyName": "Skyview Residence",
  "totalOutstanding": 770.50,
  "totalPaid": 350.00,
  "creditBalance": 0,
  "recentActivity": [
    {
      "type": "PaymentConfirmed",
      "date": "2026-06-05T00:00:00Z",
      "description": "Payment confirmed",
      "reference": "May 2026 bill",
      "amount": 350.00
    },
    {
      "type": "BillIssued",
      "date": "2026-06-01T00:00:00Z",
      "description": "Bill issued · June 2026",
      "reference": "SKV-2026-06-A0101",
      "amount": 350.00
    },
    {
      "type": "BillIssued",
      "date": "2026-05-01T00:00:00Z",
      "description": "Bill issued · May 2026",
      "reference": "SKV-2026-05-A0101",
      "amount": 350.00
    }
  ]
}
```
`totalOutstanding` is the sum of `Bill.OutstandingBalance` across the unit's bills — computed live rather
than trusted from `Account.CumulativeArrears`, since nothing in this project keeps that column in sync as
bills are added (same reasoning as `BillService.ComputeEffectiveStatus` for "Overdue"). `totalPaid` is the
sum of `Payment.Amount` for the unit's `Status = "Confirmed"` payments. `creditBalance` is
`Account.CreditBalance`, trusted as stored — nothing else in this schema computes it.

`recentActivity` merges bill-issued and payment-confirmed events by date, most recent first, capped at 3.
`type` is `"BillIssued"` or `"PaymentConfirmed"` — the frontend uses it to pick the sign/icon/color, since
the API doesn't format that presentational detail itself. `reference` is the bill's reference number for a
`BillIssued` entry, or `"{billing period} bill"` for a `PaymentConfirmed` entry.

### Payment Proof (UC-106) — implemented

`[Authorize]`, scoped to the JWT's `ResidentId` **and** `UnitId` claims. File storage is Supabase Storage
(bucket `payment-proofs`, configurable via `Supabase:StorageBucket`) — the backend uploads the file via the
Storage REST API using the `Supabase:ServiceRoleKey` service-account key (residents authenticate against
our own JWT, not Supabase Auth, so there's no per-resident Supabase session to upload under) and stores the
resulting public URL in `PaymentProof.FileUrl`. The bucket is created automatically on first upload if it
doesn't exist yet (`SupabaseStorageService.EnsureBucketExistsAsync`), public, so no manual Supabase-dashboard
setup is required beyond the service-role key.

**The ERD has no direct `PaymentProof`↔`Bill` relationship** — only `Payment`→`Bill` and `Payment`→`PaymentProof`
(both already in the schema). So "tagging" a proof to bill(s) creates one `Payment` row per tagged bill
(`Status = "Pending"`, `Amount = Bill.OutstandingBalance`, `ProofId` → the new proof) rather than needing a new
join table or migration. Each tagged bill's `Status` is also set directly to `"ProofSubmitted"` (see the Bills
section above). Because `DashboardService.TotalPaid` only sums `Status = "Confirmed"` payments, a pending proof
correctly does *not* count as paid yet — an admin flipping `Payment.Status` to `"Confirmed"` (and the bill to
`"Paid"`) on review is admin-portal work, out of scope here.

#### `POST /api/residents/payment-proofs`
> Updated for multi-file support (up to 3 files/submission) — see `docs/PROGRESS.md`'s 2026-07-15 entry for
> the full "why one `PaymentProof` row per file" reasoning. This section previously documented the original
> single-file shape; corrected here to match current code.

`multipart/form-data` request: one to three `Files` parts (repeated, e.g. `Files=<file1>&Files=<file2>`) and
one or more `BillIds` parts (repeated, e.g. `BillIds=6&BillIds=7`). Each file validated server-side (in
addition to client-side validation, which is only a UX convenience — this can't be trusted alone):
Content-Type **and** extension must both be one of `image/jpeg`/`.jpg`/`.jpeg`, `image/png`/`.png`,
`application/pdf`/`.pdf`; size ≤ 5 MB each. `BillIds` must be non-empty and every id must resolve to a bill
owned by the caller's unit (silently excludes/rejects bills belonging to another resident's unit, same
pattern as the Bill Detail 404). Response `201`:
```json
{
  "proofId": 4,
  "files": [
    { "proofId": 4, "fileUrl": "https://<project-ref>.supabase.co/storage/v1/object/public/payment-proofs/5/3f9c2b1a....png", "fileType": "image/png", "fileSize": 421890 },
    { "proofId": 5, "fileUrl": "https://<project-ref>.supabase.co/storage/v1/object/public/payment-proofs/5/8a1e77cd....png", "fileType": "image/png", "fileSize": 398120 }
  ],
  "status": "Pending",
  "adminRemarks": null,
  "submittedAt": "2026-07-10T09:00:00Z",
  "reviewedAt": null,
  "taggedBills": [
    { "billId": 8, "referenceNumber": "SKV-2026-04-A0101", "billingPeriod": "2026-04", "amount": 420.50 }
  ]
}
```
One `PaymentProof` row is created per file (fits the ERD's single-value `FileUrl`/`FileType`/`FileSize`
columns with no migration); `proofId` at the top level identifies the first-uploaded ("primary") file, and
only that file's row is linked to the tagged bills' `Payment` rows — so a 3-file submission doesn't triple
the pending amount owed on each tagged bill. `400` — no files / more than 3 files / an invalid file
(wrong type or size), no bills tagged, or a tagged bill doesn't belong to the resident's unit. `502` — the
Supabase Storage upload itself failed (network issue, bad service-role key, etc) for any file in the batch;
no partial writes (nothing is persisted until every file has uploaded successfully).

#### `GET /api/residents/payment-proofs`
Response `200` — the resident's submissions, most recent first, same shape as the array elements above (each
with its own `files` and `taggedBills`). Used for the Pay tab's submission history. Submissions are
reconstructed by grouping `PaymentProof` rows on exact `SubmittedAt` equality — the service stamps every file
in one submission with a single `DateTime.UtcNow` captured once per request, so same-instant rows are
guaranteed to be "files from the same upload" without a batch/group column the ERD doesn't have.

### Disputes (UC-108 submit, UC-110 history) — implemented

`[Authorize]`, scoped to the JWT's `ResidentId` **and** `UnitId` claims (submission needs `UnitId` to verify
the disputed bill belongs to the caller's unit; the other two only need `ResidentId`). One active dispute per
bill: a bill with an `Open` or `UnderReview` dispute already on it can't take a second one until that one is
resolved. `Dispute.AdminResponse` is **not in the Chapter 4 ERD** — added via the
`20260716095246_AddDisputeAdminResponse` migration because Figure 4.13's own caption says the dispute screen
shows "the dispute status and admin response," but the ERD's `Dispute` table has nowhere to store one (unlike
`PaymentProof`, which already has `AdminRemarks`). See `docs/SCHEMA.md`'s "Deliberate additions" section.

#### `POST /api/residents/disputes`
Request:
```json
{ "billId": 14, "reason": "The water charge of RM 32.50 looks higher than my usual monthly usage." }
```
`reason` is validated server-side via `[MinLength(20)]` (`[ApiController]`'s automatic model validation, same
pattern as `ChangePasswordRequest.NewPassword`) — `400` with the exact message
`"Please provide a dispute reason of at least 20 characters."` if too short, matching what the Submit Dispute
screen's live character counter also enforces client-side. Response `201`:
```json
{
  "disputeId": 1,
  "billId": 14,
  "billReferenceNumber": "SKV-2026-04-A0101",
  "billingPeriod": "2026-04",
  "reason": "The water charge of RM 32.50 looks higher than my usual monthly usage.",
  "status": "Open",
  "submittedAt": "2026-07-16T09:55:35Z",
  "resolvedAt": null,
  "adminResponse": null
}
```
`404` if `billId` doesn't exist **or** doesn't belong to the caller's unit (uniform 404, same pattern as Bill
Detail — doesn't confirm a `billId` belongs to someone else). `409` if the bill already has an active
(`Open`/`UnderReview`) dispute — body includes both a `message` and the existing `dispute` (same `DisputeDto`
shape as success), so the frontend can show it directly without a second fetch:
```json
{ "message": "This bill already has an active dispute. Please wait for it to be resolved.", "dispute": { "...": "DisputeDto" } }
```

#### `GET /api/residents/disputes?status={status}`
`status` query param is optional, case-insensitive, one of `Open` | `UnderReview` | `Resolved` | `Rejected` —
filters to disputes with that exact stored status (unlike Bills' `?status=`, this one *is* a raw column, not
computed, so it's a straightforward SQL-level filter). Response `200` — same shape as the `POST` response,
newest first.

#### `GET /api/residents/disputes/{id}`
Response `200` — a single `DisputeDto`. `404` if it doesn't exist **or** belongs to a different resident,
same uniform-404 pattern as everywhere else in this API.

### Notifications (UC-107) — implemented

`[Authorize]`, scoped to the JWT's `ResidentId` claim (token registration also needs no `UnitId`). Push
delivery is via the [Expo Push API](https://exp.host/--/api/v2/push/send) — a public endpoint, no Expo
account/access-token needed for basic sending. **Push notifications only actually arrive on a physical
device running an Expo dev build (or standalone build) — not in Expo Go, since SDK 53 removed remote push
support from Expo Go.** Token registration and in-app notification history both work identically in Expo Go;
only the final "does a push land on the device" step needs a dev build to observe.

#### `POST /api/residents/notification-tokens`
Request:
```json
{ "expoPushToken": "ExponentPushToken[xxxxxxxxxxxxxxxxxxxxxx]", "deviceInfo": "iPhone 15, iOS 18.1" }
```
Upserts by the token string itself (not by resident), so re-registering the same token — e.g. on every app
launch, which is the intended usage — just refreshes `RegisteredAt`/`DeviceInfo` rather than creating
duplicates. If the same token was previously registered under a *different* resident (a shared device, prior
resident logged out), it's reassigned to whoever registers it now, since only the current session on that
device should receive its pushes. Response `200`:
```json
{ "tokenId": 1, "expoPushToken": "ExponentPushToken[xxxxxxxxxxxxxxxxxxxxxx]", "isActive": true, "registeredAt": "2026-07-16T09:00:52Z" }
```

#### `GET /api/residents/notifications`
Response `200` — newest first:
```json
[
  { "notificationId": 3, "type": "BillIssued", "title": "New bill issued", "body": "A new bill has been issued for your unit. Tap to view details.", "deepLink": null, "isRead": false, "sentAt": "2026-07-16T09:27:46Z" }
]
```
`type` is one of `BillIssued` | `BillOverdue` | `PaymentConfirmed` | `DueReminder` (the four categories this
task's sending service and dev-test endpoint know about — an admin-triggered sender, when built, can send
any string here since `Type` is unconstrained free text in the schema). `deepLink`, when present, is a path
into the resident app's own router (e.g. `/(tabs)/bills/12`) for the app to navigate to on tap.

#### `PUT /api/residents/notifications/{id}/read`
Marks one notification read. `204` on success. `404` if it doesn't exist **or** belongs to a different
resident — deliberately not distinguished, same uniform-404 pattern as Bill Detail.

#### `PUT /api/residents/notifications/read-all`
Marks every unread notification for the resident as read (a set-based `ExecuteUpdateAsync`, not a fetch-then-
loop). `204` on success, always — a no-op if there was nothing unread.

#### Sending (internal — no dedicated endpoint)
`INotificationSendingService.SendAsync(residentId, type, title, body, deepLink)` is what actually creates a
`Notification` row and pushes to the resident's active tokens; nothing outside this task calls it yet (the
admin module that would call it on bill-issue/overdue/payment-confirm events doesn't exist), so
`POST /api/dev/notifications/test` (below) is currently its only caller. Preference handling:
- `NotificationPreferences.BillDueReminders == false` and `type == "DueReminder"` → **nothing is sent at
  all** — no `Notification` row, no push. This is the one category with its own dedicated toggle, so
  "respecting the preference" means suppressing it outright.
- `NotificationPreferences.PushEnabled == false` → the `Notification` row is still created (the resident can
  still see it in their in-app Alerts history) but no Expo push is attempted — `PushEnabled` gates the device
  push channel specifically, not whether the event gets recorded.
- `EmailEnabled` is parsed but unused — no email-sending infrastructure exists in this project; out of scope
  for this task.

Delivery failures are logged (`ILogger<ExpoPushService>`/`ILogger<NotificationSendingService>`) with the
Expo ticket's error code. A ticket whose `details.error` is `"DeviceNotRegistered"` deactivates that token
(`NotificationToken.IsActive = false`) so it's excluded from future sends — verified live: a fake token
correctly comes back `DeviceNotRegistered` and gets deactivated in the same request.

#### `POST /api/dev/notifications/test` — Development environment only
**Not part of the Module A/B contract** — a standalone dev tool for demoing/testing notifications without
the (not yet built) admin module. `DevController` is mapped the same as every other controller (nothing
excludes it from routing based on environment), so the actual safeguard is inside the action itself: it
checks `IWebHostEnvironment.IsDevelopment()` first and returns `404` immediately if the app isn't running in
Development — this is the only thing preventing it from working in Production, so it must not be removed.
Not `[Authorize]` — takes a raw `residentId` in the body since it's meant to be curled directly during
development, not driven from a logged-in session. Request:
```json
{ "residentId": 9, "type": "BillIssued", "title": null, "body": null, "deepLink": null }
```
`type` defaults to `"BillIssued"`; `title`/`body` default to a canned sample per `type` (matching Figure
4.14's four example notifications) when omitted; `deepLink` is optional. Response `200`
`{ "message": "Notification sent." }`, or `404` if `residentId` doesn't exist.

### Digital Receipt (UC-109) — implemented

`[Authorize]`, scoped to the JWT's `UnitId` claim — same unit-scoping as Bills/Dashboard rather than
resident-scoping like Disputes/Payment Proof, since a receipt is fundamentally tied to a `Payment` → `Bill` →
`Unit` chain with no `ResidentId` of its own in the ERD (co-residents on one unit would reasonably see the
same payment history, same as they already see the same bills). Only `Payment` rows with `Status ==
"Confirmed"` are ever surfaced — a `Pending` payment (e.g. one created by tagging a Payment Proof, per UC-106)
never appears here or gets a receipt number, since nothing has actually confirmed it was received yet.

`ReceiptNumber` (`"RCPT-{PaymentId:D6}"`, e.g. `"RCPT-000009"`) is **synthetic, not a stored column** —
`Payment` has no reference-number field in the ERD, so this is derived on every response rather than
persisted. PDF generation is on-device (`expo-print`, same as UC-105's Bill PDF — no server rendering), so
`ReceiptDetailDto` only adds `unitNumber`/`propertyName` beyond the list shape (for the PDF masthead); it
deliberately does **not** include the resident's name — the frontend already has it from `AuthContext`, the
same source `generateAndShareBillPdf` already uses for the Bill PDF's "Billed To" line.

#### `GET /api/residents/receipts`
Response `200` — the unit's confirmed payments, most recent first:
```json
[
  {
    "paymentId": 9,
    "receiptNumber": "RCPT-000009",
    "amount": 350.00,
    "paymentDate": "2026-06-05T00:00:00Z",
    "channel": "Bank Transfer",
    "billReferenceNumber": "SKV-2026-05-A0101",
    "billingPeriod": "2026-05"
  }
]
```

#### `GET /api/residents/receipts/{paymentId}`
Response `200` — same fields as above plus `unitNumber`/`propertyName`. `404` if the payment doesn't exist,
belongs to a different unit, or exists but isn't `Confirmed` — all three cases collapse to the same uniform
`404`, same non-disclosure pattern as Bill Detail and Disputes.
