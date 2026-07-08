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
