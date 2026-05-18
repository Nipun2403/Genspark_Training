# Business Rules

## 1. Membership Rules

### 1.1 Membership Types
All limits for memberships are stored in a **database configuration table** (`MembershipConfig`) 

| Membership Type | Max Active Borrowings | Max Borrow Days |
|---|---|---|
| Basic | 2 | 7 |
| Student | 3 | 10 |
| Premium | 5 | 15 |

* **Why a config table?** If we decide to add a new membership tier (e.g., "Platinum") or change the borrow limit for Students from 3 to 4, we can update a single database row instead of redeploying the application. An attempt to make the library system as **future-proof** as possible.

### 1.2 Membership Status
-  **Active** or **Inactive**.
- Only **Active** members are allowed to borrow books.
- Deactivating a member does **not** erase their borrowing history or pending fines. It only prevents new borrowings.

### 1.3 Member Identification
- Each member has a unique **MemberId** (auto-generated integer).
- Members can be searched by **Phone Number** or **Email**, both of which should be unique.

---

## 2. Book & Book Copy Rules

### 2.1 Book Identification — ISBN
- Every book title is uniquely identified by its **ISBN** (International Standard Book Number).
- The `ISBN` field serves as the **Primary Key** of the `Book` table.
- A single ISBN can have **multiple physical copies** tracked in the `BookCopy` table.

### 2.2 Book Categories
- Each book belongs to exactly **one category** (e.g., Fiction, Science, History).
- Categories are stored in the `BookCategory` table.

### 2.3 Book Copy Status
A book copy can be in one of the following statuses:

| Status | Meaning |
|---|---|
| `Available` | The copy is on the shelf and can be borrowed. |
| `Borrowed` | The copy is currently lent out to a member. |
| `MinorDamage` | The copy has minor damage but is still usable and can be borrowed. |
| `DamagedBeyondUsable` | The copy is severely damaged and **cannot** be borrowed. |
| `Lost` | The copy has been reported lost by a member. |

---

## 3. Borrowing Rules

### 3.1 Pre-Borrowing Validation (All Must Pass)
Before a member can borrow a book copy, the system checks **all** of the following conditions. If **any** condition fails, the borrowing is **rejected** and the transaction is **rolled back**.

| # | Validation | Failure Message |
|---|---|---|
| 1 | Member exists and is **Active** | "Member not found or is inactive." |
| 2 | Member's total unpaid fines ≤ ₹500 | "Cannot borrow. Unpaid fines exceed ₹500." |
| 3 | Member's active borrowings < their membership limit | "Borrowing limit reached for your membership type." |
| 4 | The requested book copy status is `Available` or `MinorDamage` | "This book copy is not available for borrowing." |
| 5 | Member does **not** already have an active (unreturned) borrowing for the **same ISBN** | "You already have an active borrowing for this book." |

### 3.2 Borrowing Transaction Steps
The entire borrowing process runs inside a **single database transaction**:

1. Validate member (active status).
2. Validate unpaid fines (≤ ₹500).
3. Check active borrowing count against membership limit.
4. Check book copy availability.
5. Check for duplicate active borrowing of the same ISBN.
6. Create a new `Borrowing` record with `BorrowDate = today` and `DueDate = today + MaxBorrowDays`.
7. Update the book copy status to `Borrowed`.
8. **Commit** the transaction.

If **any step fails**, the entire transaction is **rolled back** — no partial data is saved.

### 3.3 Due Date Calculation
- `DueDate = BorrowDate + MaxBorrowDays` (based on the member's membership type config).
- Example: A Student borrows on Jan 1 → Due Date = Jan 11 (10 days).

---

## 4. Return Rules

### 4.1 Return Process
When a member returns a book:

1. Find the active borrowing record for the member + book copy.
2. Set `ReturnDate = today`.
3. Mark the borrowing status as `Returned`.
4. Calculate any **late return fine** (see §5.1).
5. Assess **damage** (see §4.2).
6. Update the book copy status accordingly.

### 4.2 Damage Assessment on Return
The librarian selects one of three options when accepting a return:

| Option | Action |
|---|---|
| **No Damage** | Book copy status → `Available`. No damage fine. |
| **Minor Damage** | Book copy status → `MinorDamage`. A flat fine is charged **only if** the copy was `Available` (good condition) when it was borrowed. If it was already `MinorDamage` when borrowed, **no damage fine** is charged. |
| **Damaged Beyond Usable** | Book copy status → `DamagedBeyondUsable`. A higher flat fine is charged **only if** the copy was `Available` or `MinorDamage` when borrowed. The copy is **retired** from lending. |

> **Key Insight — The "Already Damaged" Rule:**
> If a member borrows a copy that was already in `MinorDamage` status and returns it in the same condition, they are **not** fined for damage. The system tracks the condition of the copy **at the time of borrowing** (`ConditionAtBorrow`) to make this comparison.

### 4.3 Lost Book
- If a member reports a book as lost, the book copy status is set to `Lost`.
- A **lost book fine** is charged (the highest fine category).
- The borrowing is marked as `Returned` with a note indicating the book was lost.

---

## 5. Fine Rules

### 5.1 Fine Types & Amounts
Fine amounts are stored in the **database configuration** for future-proofing.

| Fine Type | Default Amount | Trigger |
|---|---|---|
| **Late Return** | ₹10 per day overdue | `ReturnDate > DueDate` |
| **Minor Damage** | Flat fine (e.g., ₹200) | Copy returned with minor damage when it was borrowed in good condition |
| **Damaged Beyond Usable** | Higher flat fine (e.g., ₹500) | Copy returned severely damaged |
| **Lost Book** | Highest fine (e.g., ₹1000) | Member reports the book as lost |

> Fine amounts listed above are **defaults**. The actual values are read from the `FineConfig` table in the database so they can be adjusted without code changes.

### 5.2 Fine Blocking Rule
- A member **cannot borrow** any new books if their total **unpaid fines exceed ₹500**.
- The ₹500 threshold is also stored in the config table.

### 5.3 Fine Payment
- A member can pay their fines (full or partial payment).
- Each payment is recorded in the `FinePayment` table with a timestamp.
- A fine is considered **paid** when its remaining balance reaches ₹0.

### 5.4 Fine History
- The system maintains a complete history of all fines and payments for audit and reporting.

---

## 6. Report Rules

The system provides the following reports:

| Report | Description |
|---|---|
| Books Currently Borrowed | All borrowings where `ReturnDate IS NULL` |
| Overdue Books | Active borrowings where `DueDate < today` |
| Members with Pending Fines | Members whose total unpaid fines > ₹0 |
| Most Borrowed Books | Books ranked by total borrowing count |
| Available Books by Category | Book copies with `Available` or `MinorDamage` status, grouped by category |
| Member Borrowing History | All borrowings (active + returned) for a specific member |

---

## 7. PostgreSQL Function Rules

At least one PostgreSQL function must be created and called from EF Core:

1. **`calculate_member_fine(member_id)`** — Returns the total unpaid fine amount for a given member.
2. **`get_available_books_by_category(category_id)`** — Returns available book copies under a specific category.
3. **`get_member_borrowing_summary(member_id)`** — Returns active borrowings, returned borrowings, and total fine for a member.

These functions encapsulate read-heavy reporting logic at the database level for performance and consistency.
