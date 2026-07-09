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
| Receipts | `/api/residents/receipts/{paymentId}` | GET | Digital receipt data for confirmed payments |

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
`multipart/form-data` request: a `File` part (the proof image/PDF) and one or more `BillIds` parts (repeated,
e.g. `BillIds=6&BillIds=7`). Validated server-side (in addition to client-side validation, which is only a UX
convenience — this can't be trusted alone): Content-Type **and** extension must both be one of
`image/jpeg`/`.jpg`/`.jpeg`, `image/png`/`.png`, `application/pdf`/`.pdf`; size ≤ 5 MB. `BillIds` must be
non-empty and every id must resolve to a bill owned by the caller's unit (silently excludes/rejects bills
belonging to another resident's unit, same pattern as the Bill Detail 404). Response `201`:
```json
{
  "proofId": 4,
  "fileUrl": "https://<project-ref>.supabase.co/storage/v1/object/public/payment-proofs/5/3f9c2b1a....png",
  "fileType": "image/png",
  "fileSize": 421890,
  "status": "Pending",
  "adminRemarks": null,
  "submittedAt": "2026-07-10T09:00:00Z",
  "reviewedAt": null,
  "taggedBills": [
    { "billId": 8, "referenceNumber": "SKV-2026-04-A0101", "billingPeriod": "2026-04", "amount": 420.50 }
  ]
}
```
`400` — invalid file (wrong type/size), no bills tagged, or a tagged bill doesn't belong to the resident's
unit. `502` — the Supabase Storage upload itself failed (network issue, bad service-role key, etc).

#### `GET /api/residents/payment-proofs`
Response `200` — the resident's submissions, most recent first, same shape as the array elements above (each
with its own `taggedBills`). Used for the Pay tab's submission history.
