## Table Design

### 1. `book_categories`

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `category_id` | `SERIAL` | PK | Auto-increment |
| `category_name` | `VARCHAR(100)` | NOT NULL, UNIQUE | e.g., "Fiction", "Science" |

---

###  2.`books`

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `isbn` | `VARCHAR(20)` | PK | ISBN as natural primary key |
| `title` | `VARCHAR(250)` | NOT NULL | Book title |
| `author` | `VARCHAR(200)` | NOT NULL | Author name |
| `category_id` | `INT` | FK → `book_categories`, NOT NULL | Category reference |
| `created_at` | `TIMESTAMP` | NOT NULL, DEFAULT NOW() | Record creation time |

**Index:** `idx_books_title` on `title` for search performance.
**Index:** `idx_books_author` on `author` for search performance.

---

### 3. `book_copies`

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `copy_id` | `SERIAL` | PK | Auto-increment |
| `isbn` | `VARCHAR(20)` | FK → `books`, NOT NULL | Which book this copy belongs to |
| `status` | `VARCHAR(30)` | NOT NULL, DEFAULT 'Available' | `Available`, `Borrowed`, `MinorDamage`, `DamagedBeyondUsable`, `Lost` |
| `created_at` | `TIMESTAMP` | NOT NULL, DEFAULT NOW() | When this copy was added |

**Check Constraint:** `status IN ('Available', 'Borrowed', 'MinorDamage', 'DamagedBeyondUsable', 'Lost')`

---

### 4. `members`

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `member_id` | `SERIAL` | PK | Auto-increment |
| `full_name` | `VARCHAR(200)` | NOT NULL | Member's full name |
| `email` | `VARCHAR(200)` | NOT NULL, UNIQUE | For search |
| `phone_number` | `VARCHAR(20)` | NOT NULL, UNIQUE | For search |
| `membership_type` | `VARCHAR(20)` | NOT NULL, FK → `membership_config` | `Basic`, `Student`, `Premium` |
| `is_active` | `BOOLEAN` | NOT NULL, DEFAULT TRUE | Active/Inactive toggle |
| `join_date` | `TIMESTAMP` | NOT NULL, DEFAULT NOW() | Registration date |

**Index:** `idx_members_email` on `email`.
**Index:** `idx_members_phone` on `phone_number`.

---

### 5. `membership_config`

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `config_id` | `SERIAL` | PK | Auto-increment |
| `membership_type` | `VARCHAR(20)` | NOT NULL, UNIQUE | `Basic`, `Student`, `Premium` |
| `max_active_borrowings` | `INT` | NOT NULL | Borrowing limit |
| `max_borrow_days` | `INT` | NOT NULL | Max days before overdue |

> **Purpose:**  Source of truth for membership limits and all. All the details are referenced from here regarding the limits and all.

---

### 6. `borrowings`

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `borrowing_id` | `SERIAL` | PK | Auto-increment |
| `member_id` | `INT` | FK → `members`, NOT NULL | Who borrowed |
| `copy_id` | `INT` | FK → `book_copies`, NOT NULL | Which copy |
| `borrow_date` | `TIMESTAMP` | NOT NULL, DEFAULT NOW() | When borrowed |
| `due_date` | `TIMESTAMP` | NOT NULL | Calculated: `borrow_date + max_borrow_days` |
| `return_date` | `TIMESTAMP` | NULLABLE | Null while active |
| `status` | `VARCHAR(20)` | NOT NULL, DEFAULT 'Active' | `Active` or `Returned` |
| `condition_at_borrow` | `VARCHAR(30)` | NOT NULL | Copy status when borrowed (`Available` or `MinorDamage`) |
| `condition_at_return` | `VARCHAR(30)` | NULLABLE | Set on return: `NoDamage`, `MinorDamage`, `DamagedBeyondUsable`, `Lost` |

---

### 7. `fines`

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `fine_id` | `SERIAL` | PK | Auto-increment |
| `member_id` | `INT` | FK → `members`, NOT NULL | Who owes |
| `borrowing_id` | `INT` | FK → `borrowings`, NOT NULL | Which borrowing caused it |
| `fine_type` | `VARCHAR(30)` | NOT NULL | `LateReturn`, `MinorDamage`, `DamagedBeyondUsable`, `Lost` |
| `amount` | `DECIMAL(10,2)` | NOT NULL | Total fine amount |
| `paid_amount` | `DECIMAL(10,2)` | NOT NULL, DEFAULT 0 | Amount paid so far |
| `is_paid` | `BOOLEAN` | NOT NULL, DEFAULT FALSE | True when `paid_amount >= amount` |
| `created_at` | `TIMESTAMP` | NOT NULL, DEFAULT NOW() | When the fine was created |

---

### 8. `fine_payments`

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `payment_id` | `SERIAL` | PK | Auto-increment |
| `fine_id` | `INT` | FK → `fines`, NOT NULL | Which fine is being paid |
| `amount_paid` | `DECIMAL(10,2)` | NOT NULL | Amount of this payment |
| `payment_date` | `TIMESTAMP` | NOT NULL, DEFAULT NOW() | When payment was made |

---

### 9. `fine_config`

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `fine_config_id` | `SERIAL` | PK | Auto-increment |
| `fine_type` | `VARCHAR(30)` | NOT NULL, UNIQUE | `LateReturn`, `MinorDamage`, `DamagedBeyondUsable`, `Lost` |
| `amount` | `DECIMAL(10,2)` | NOT NULL | ₹10/day for LateReturn, flat for others |
| `max_unpaid_fine_threshold` | `DECIMAL(10,2)` | NULLABLE | ₹500 threshold (only on one row, or global) |

> **Purpose:**  Source of truth for types of fine and their amount.

> **Seed Data:**

| fine_type | amount | max_unpaid_fine_threshold |
|---|---|---|
| `LateReturn` | 10.00 | 500.00 |
| `MinorDamage` | 200.00 | NULL |
| `DamagedBeyondUsable` | 500.00 | NULL |
| `Lost` | 1000.00 | NULL |

---
