# ER Diagram — Library System

## Entity Relationship Diagram

```mermaid
erDiagram
    BookCategory ||--o{ Book : "has many"
    Book ||--o{ BookCopy : "has many copies"
    Member ||--o{ Borrowing : "borrows"
    BookCopy ||--o{ Borrowing : "is borrowed in"
    Member ||--o{ Fine : "owes"
    Borrowing ||--o{ Fine : "generates"
    Fine ||--o{ FinePayment : "paid via"
    MembershipConfig ||--o{ Member : "defines limits for"

    BookCategory {
        int CategoryId PK
        string CategoryName
    }

    Book {
        string ISBN PK
        string Title
        string Author
        int CategoryId FK
        datetime CreatedAt
    }

    BookCopy {
        int CopyId PK
        string ISBN FK
        string Status "Available | Borrowed | MinorDamage | DamagedBeyondUsable | Lost"
        datetime CreatedAt
    }

    Member {
        int MemberId PK
        string FullName
        string Email UK
        string PhoneNumber UK
        string MembershipType FK "Basic | Student | Premium"
        bool IsActive
        datetime JoinDate
    }

    MembershipConfig {
        int ConfigId PK
        string MembershipType UK "Basic | Student | Premium"
        int MaxActiveBorrowings
        int MaxBorrowDays
    }

    Borrowing {
        int BorrowingId PK
        int MemberId FK
        int CopyId FK
        datetime BorrowDate
        datetime DueDate
        datetime ReturnDate "nullable"
        string Status "Active | Returned"
        string ConditionAtBorrow "Available | MinorDamage"
        string ConditionAtReturn "nullable: NoDamage | MinorDamage | DamagedBeyondUsable | Lost"
    }

    Fine {
        int FineId PK
        int MemberId FK
        int BorrowingId FK
        string FineType "LateReturn | MinorDamage | DamagedBeyondUsable | Lost"
        decimal Amount
        decimal PaidAmount
        bool IsPaid
        datetime CreatedAt
    }

    FinePayment {
        int PaymentId PK
        int FineId FK
        decimal AmountPaid
        datetime PaymentDate
    }

    FineConfig {
        int FineConfigId PK
        string FineType UK "LateReturn | MinorDamage | DamagedBeyondUsable | Lost"
        decimal Amount "Per-day for LateReturn or flat for others"
        decimal MaxUnpaidFineThreshold "Only for blocking borrowing"
    }
```

---

## Entity Summary Table

| Entity | PK | Description |
|---|---|---|
| `BookCategory` | `CategoryId` (int, auto) | Lookup table for book genres/categories. |
| `Book` | `ISBN` (string) | Represents a book title. One ISBN = one book title. |
| `BookCopy` | `CopyId` (int, auto) | A physical copy of a book. Multiple copies per ISBN. |
| `Member` | `MemberId` (int, auto) | A library member with a membership type. |
| `MembershipConfig` | `ConfigId` (int, auto) | Config table storing borrowing limits per membership type. |
| `Borrowing` | `BorrowingId` (int, auto) | A single borrowing transaction linking a Member to a BookCopy. |
| `Fine` | `FineId` (int, auto) | A fine record generated from a borrowing (late, damage, or lost). |
| `FinePayment` | `PaymentId` (int, auto) | Individual payment against a Fine. Supports partial payments. |
| `FineConfig` | `FineConfigId` (int, auto) | Config table storing fine amounts by type. Future-proof. |

---

## Relationships

| Relationship | Type | Description |
|---|---|---|
| `BookCategory` → `Book` | One-to-Many | Each category contains many books. |
| `Book` → `BookCopy` | One-to-Many | Each ISBN has multiple physical copies. |
| `Member` → `Borrowing` | One-to-Many | A member can have many borrowings over time. |
| `BookCopy` → `Borrowing` | One-to-Many | A copy can be borrowed multiple times (sequentially). |
| `Borrowing` → `Fine` | One-to-Many | A single borrowing can generate multiple fines (e.g., late + damage). |
| `Fine` → `FinePayment` | One-to-Many | A fine can be paid in multiple installments. |
| `MembershipConfig` → `Member` | One-to-Many | Config defines limits for all members of that type. |
