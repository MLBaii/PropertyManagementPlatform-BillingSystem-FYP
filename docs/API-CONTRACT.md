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
| Account | `/api/residents/account` | GET | Dashboard data, running balance, transaction history |
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
Raw `Bill.Status` in the database only ever holds `"Unpaid"` or `"Paid"` (plus, once payment-proof upload
exists, `"ProofSubmitted"`).

#### `GET /api/residents/bills?status={status}`
`status` query param is optional, case-insensitive, one of `Unpaid` | `Overdue` | `Paid` (and, once
implemented, `ProofSubmitted`) — filters the list to bills whose *computed* status matches. Response `200`:
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
Response `200` — same fields as the list, plus the itemised line items:
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
  ]
}
```
`404` if the bill doesn't exist **or** belongs to a different unit — deliberately not distinguished, to avoid
confirming a `billId` belongs to someone else.
