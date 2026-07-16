# Database Schema (Shared)

> Designed jointly. Single PostgreSQL (hosted on Supabase) database.
> Resident-owned entities and admin-owned entities share this schema.
> Backend connects via EF Core using the Npgsql.EntityFrameworkCore.PostgreSQL provider.

This reflects the exact Chapter 4 ERD — rebuilt from an earlier simplified version that had
drifted from the documented design (see `docs/PROGRESS.md`, 2026-07-08 "Rebuild schema to
match Chapter 4 ERD exactly" entry for what changed and why).

## Entities (from §4.4)
Resident-owned: Resident, PaymentProof, Dispute, Notification, NotificationToken
Shared with admin: Property, Unit, Account, Bill, BillLineItem, Payment
Admin-owned: BillingItem, UnitBillingRate, AdminUser, AuditLog

## Deliberate additions vs. the ERD
`Resident.IsActive` (boolean) is **not** in the Chapter 4 ERD. It was added to support UC-101
(resident login) returning HTTP 403 for a disabled account — the ERD has no other column that
captures that state. Flagged here for disclosure in Chapter 5/7.

`Dispute.AdminResponse` (text, nullable) is **not** in the Chapter 4 ERD either. Added to support
UC-110 (view dispute history) — Figure 4.13's own mockup caption says the dispute screen shows
"the dispute status and admin response," but the ERD's `Dispute` table has no column for one
(unlike `PaymentProof`, which already has `AdminRemarks`). Same disclosure category as above.

## Key relationships
- Property 1—* Unit
- Property 1—* BillingItem
- Unit 1—* Resident (co-owners/tenants possible)
- Unit 1—* Bill
- Unit 1—* UnitBillingRate
- Resident 1—1 Account (Resident holds the `AccountId` FK; enforced unique so each Account belongs to exactly one Resident)
- Resident 1—* PaymentProof, 1—* Dispute, 1—* Notification, 1—* NotificationToken
- Bill 1—* BillLineItem, 1—* Payment, 1—* Dispute
- BillLineItem *—1 BillingItem (optional — a line item need not reference a catalogue entry)
- Payment *—1 PaymentProof (optional — `Payment.ProofId` is nullable; a payment isn't required to be backed by an uploaded proof, and this keeps Payment/PaymentProof from forming a hard mutual dependency)
- BillingItem 1—* UnitBillingRate
- AdminUser 1—* AuditLog

## Table definitions

### Property
| Column | Type | Notes |
|---|---|---|
| PropertyId | int | PK |
| Name | text | |
| Address | text | |
| ContactEmail | text | |
| ContactPhone | text | |
| CreatedAt | timestamptz | |

### Unit
| Column | Type | Notes |
|---|---|---|
| UnitId | int | PK |
| PropertyId | int | FK → Property |
| UnitNumber | text | |
| Floor | int | |
| Type | text | e.g. "Studio", "3-Bedroom" |
| IsActive | bool | |

### Resident
| Column | Type | Notes |
|---|---|---|
| ResidentId | int | PK |
| UnitId | int | FK → Unit |
| AccountId | int | FK → Account, unique (1—1) |
| Email | text | unique |
| PasswordHash | text | bcrypt |
| Name | text | |
| PhoneNumber | text | nullable |
| NotificationPreferences | text | JSON-encoded |
| CreatedAt | timestamptz | |
| **IsActive** | bool | **not in the ERD** — added for UC-101's 403 case |

### Account
| Column | Type | Notes |
|---|---|---|
| AccountId | int | PK |
| CumulativeArrears | numeric | |
| CreditBalance | numeric | |
| LastUpdated | timestamptz | |

Note: Account has no FK columns of its own — it's the principal side of the Resident 1—1
relationship (Resident holds `AccountId`). Unlike the earlier simplified schema, Account is no
longer tied to Unit directly.

### Bill
| Column | Type | Notes |
|---|---|---|
| BillId | int | PK |
| UnitId | int | FK → Unit |
| BillingPeriod | text | e.g. "2026-06" |
| ReferenceNumber | text | unique |
| IssueDate | timestamptz | |
| TotalAmount | numeric | |
| OutstandingBalance | numeric | |
| Status | text | e.g. "Pending", "Paid" |
| DueDate | timestamptz | |

### BillLineItem
| Column | Type | Notes |
|---|---|---|
| LineItemId | int | PK |
| BillId | int | FK → Bill |
| BillingItemId | int | FK → BillingItem, nullable |
| Description | text | |
| Amount | numeric | |
| LineItemType | text | e.g. "Charge", "Penalty" |

### Payment
| Column | Type | Notes |
|---|---|---|
| PaymentId | int | PK |
| BillId | int | FK → Bill |
| ProofId | int | FK → PaymentProof, nullable |
| Amount | numeric | |
| PaymentDate | timestamptz | |
| Channel | text | e.g. "Bank Transfer", "Card" |
| Status | text | |

### PaymentProof
| Column | Type | Notes |
|---|---|---|
| ProofId | int | PK |
| ResidentId | int | FK → Resident |
| FileUrl | text | |
| FileType | text | |
| FileSize | bigint | bytes |
| Status | text | |
| AdminRemarks | text | nullable |
| SubmittedAt | timestamptz | |
| ReviewedAt | timestamptz | nullable |

Note: a proof is submitted by a Resident directly, not scoped to one Bill — admins tag it to
the relevant Payment(s) during review.

### Dispute
| Column | Type | Notes |
|---|---|---|
| DisputeId | int | PK |
| BillId | int | FK → Bill |
| ResidentId | int | FK → Resident |
| Reason | text | |
| Status | text | "Open" \| "UnderReview" \| "Resolved" \| "Rejected" |
| SubmittedAt | timestamptz | |
| ResolvedAt | timestamptz | nullable |
| AdminResponse | text | nullable — not in the ERD, see "Deliberate additions" above |

### Notification
| Column | Type | Notes |
|---|---|---|
| NotificationId | int | PK |
| ResidentId | int | FK → Resident |
| Type | text | |
| Title | text | |
| Body | text | |
| DeepLink | text | nullable |
| IsRead | bool | |
| SentAt | timestamptz | |

### NotificationToken
| Column | Type | Notes |
|---|---|---|
| TokenId | int | PK |
| ResidentId | int | FK → Resident |
| ExpoPushToken | text | |
| DeviceInfo | text | |
| IsActive | bool | |
| RegisteredAt | timestamptz | |

### BillingItem
| Column | Type | Notes |
|---|---|---|
| BillingItemId | int | PK |
| PropertyId | int | FK → Property |
| ChargeType | text | e.g. "Maintenance Fee" |
| DefaultRate | numeric | |
| Frequency | text | e.g. "Monthly" |
| BillingDay | int | day of month a bill is generated |
| DueDay | int | day of month payment is due |
| PenaltyRate | numeric | late-payment penalty rate |
| GracePeriodDays | int | |
| IsActive | bool | |

### UnitBillingRate
| Column | Type | Notes |
|---|---|---|
| UnitBillingRateId | int | PK |
| UnitId | int | FK → Unit |
| BillingItemId | int | FK → BillingItem |
| Rate | numeric | per-unit override of `BillingItem.DefaultRate` |

### AdminUser
| Column | Type | Notes |
|---|---|---|
| AdminUserId | int | PK |
| Username | text | unique |
| PasswordHash | text | bcrypt |
| Email | text | unique |
| Role | text | |
| CreatedAt | timestamptz | |

### AuditLog
| Column | Type | Notes |
|---|---|---|
| AuditLogId | int | PK |
| AdminUserId | int | FK → AdminUser |
| ActionType | text | |
| AffectedEntity | text | e.g. "Bill", "Resident" |
| AffectedEntityId | int | nullable — PK of the affected record |
| Description | text | nullable |
| Timestamp | timestamptz | |
