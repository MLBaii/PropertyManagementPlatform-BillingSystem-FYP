# API Contract (Shared)

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
_To be filled in per endpoint as agreed (request body + response)._
