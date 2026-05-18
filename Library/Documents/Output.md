# Library System Console Output

### 1. Main Menu
The entry point of the application.

```text
=== COMMUNITY LIBRARY SYSTEM ===
1. Member Management
2. Book Management
3. Borrow Book
4. Return Book
5. Fine Management
6. Reports
0. Exit
Select: 3
```

---

### 2. Adding a Member (Input Validation & Benefits Display)
Demonstrates strict input validation rejecting bad emails/names and dynamically displaying membership tier limits before confirming.

```text
--- Add New Member ---
Full Name: John123
  [Error] Name cannot contain numbers or special characters. Please try again.
Full Name: John Doe
Email: johndoe
  [Error] Invalid email format. Please include an '@' and a domain (e.g., test@example.com).
Email: john@example.com
Phone Number: 987654321

  [Error] Invalid phone number. Must contain only digits and be between 10 and 15 digits long.
Phone Number: 9876543210

Options:
--------------------------------------------------
1. Basic      | Max Borrows: 2  | Max Days: 7  
2. Student    | Max Borrows: 3  | Max Days: 10 
3. Premium    | Max Borrows: 5  | Max Days: 15 
--------------------------------------------------
Select Membership Type: 3

Member 'John Doe' added successfully with ID: 4
```

---

### 3. Deactivating a Member (Failure: Active Borrowing Constraint)
The system prevents a librarian from deactivating a member if they currently possess library books.

```text
--- MEMBER MANAGEMENT ---
5. Deactivate Member
Select: 5

Options:
--------------------------------------------------
1. John Doe (john@example.com)
2. Jane Smith (jane@example.com)
--------------------------------------------------
Select member to deactivate: 1

Error: Can't deactivate, currently has book: 'The Great Gatsby' (Copy #14)
```

---

### 4. Deactivating a Member (Success)
The member has returned all books and can be successfully deactivated.

```text
Options:
--------------------------------------------------
1. Jane Smith (jane@example.com)
--------------------------------------------------
Select member to deactivate: 1

Member 2 (Jane Smith) has been deactivated.
```

---

### 5. Reactivating a Member (Success)
A deactivated member comes back to the library. The system dynamically filters lists to only show inactive members.

```text
--- MEMBER MANAGEMENT ---
6. Reactivate Member
Select: 6

Options:
--------------------------------------------------
1. Jane Smith (jane@example.com)
--------------------------------------------------
Select member to reactivate: 1

Member 2 (Jane Smith) has been successfully reactivated! Welcome back!
```

---

### 6. Adding a Book Category (Duplicate Error)
The system prevents duplicate categories from being added.

```text
--- Add Category ---
Category Name: Fiction
Error: Category 'Fiction' already exists.

--- Add Category ---
Category Name: Fantasy
Category 'Fantasy' added successfully with ID: 6.
```

---

### 7. Adding a New Book (Dynamic Category Selection)
Demonstrates associating a new book with an existing category via list selection.

```text
--- Add New Book ---
ISBN: 978-0553103540
Title: A Game of Thrones
Author: George R. R. Martin

Options:
--------------------------------------------------
1. Fiction
2. Science
3. History
4. Fantasy
--------------------------------------------------
Select Category: 4

Book 'A Game of Thrones' added successfully!
```

---

### 8. Adding Copies of a Book
Adding physical copies for a specific title in the system.

```text
--- Add Copies ---
Options:
--------------------------------------------------
1. 1984 (ISBN: 978-0451524935)
2. The Great Gatsby (ISBN: 978-0743273565)
3. A Game of Thrones (ISBN: 978-0553103540)
--------------------------------------------------
Select Book: 3

Number of copies to add for 'A Game of Thrones': -5
  [Error] Please enter a valid whole number (minimum: 1).
Number of copies to add for 'A Game of Thrones': 3

Successfully added 3 copies for ISBN 978-0553103540.
```

---

### 9. Marking Copy Status (Hierarchical Selection)
Demonstrates the librarian locating a specific physical copy of a book to mark it as damaged.

```text
--- Mark Copy Status ---
Options:
--------------------------------------------------
1. 1984 (ISBN: 978-0451524935)
--------------------------------------------------
Select Book: 1

Options:
--------------------------------------------------
1. Copy #1 - Current Status: Available
2. Copy #2 - Current Status: Borrowed
3. Copy #3 - Current Status: Available
--------------------------------------------------
Select Copy: 3

New Status Options:
--------------------------------------------------
1. Available
2. MinorDamage
3. DamagedBeyondUsable
4. Lost
--------------------------------------------------
Select new status: 2

Copy #3 status updated to 'MinorDamage'.
```

---

### 10. Viewing Books Inventory (Availability Ratio)
Showcasing the precise availability ratio metrics.

```text
--- BOOK MANAGEMENT ---
4. View Books Inventory
Select: 4

ISBN               Title                          Author               Category        Copies (Avail/Total)
---------------------------------------------------------------------------------------------------------
978-0451524935     1984                           George Orwell        Fiction         2/3
978-0743273565     The Great Gatsby               F. Scott Fitzgerald  Fiction         0/2
978-0553380163     A Brief History of Time        Stephen Hawking      Science         1/1
978-0553103540     A Game of Thrones              George R. R. Martin  Fantasy         3/3
```

---

### 11. Borrowing a Book (Success)
A flawless transactional checkout process.

```text
--- BORROW A BOOK ---

Options:
--------------------------------------------------
1. John Doe (john@example.com)
--------------------------------------------------
Select Member: 1

Options:
--------------------------------------------------
1. 1984                                | by George Orwell       [2 copies avail]
2. A Game of Thrones                   | by George R. R. Martin [3 copies avail]
--------------------------------------------------
Select a book to borrow: 2

Selected Book: 'A Game of Thrones'
Automatically assigning the first available Copy #16...

Book borrowed successfully!
  Book: A Game of Thrones (Copy #16)
  Borrowed by: John Doe (ID: 1)
  Due date: 2026-06-02
```

---

### 12. Borrowing a Book (Failure: Unpaid Fines Exceed Limit)
The EF Core transaction automatically rolls back if the user has an unpaid balance over ₹500.

```text
Select Member: 2

Options:
--------------------------------------------------
1. 1984                                | by George Orwell       [2 copies avail]
--------------------------------------------------
Select a book to borrow: 1

Selected Book: '1984'
Automatically assigning the first available Copy #1...

Error: Cannot borrow. Unpaid fines (₹540.00) exceed the maximum allowed (₹500.00).
```

---

### 13. Borrowing a Book (Failure: Max Borrow Limit Reached)
Basic members can only hold 2 books at once. The transaction is aborted.

```text
Select Member: 3

Current active borrowings (2):
  - 1984 (Copy #1) | Due: 2026-05-20
  - A Brief History of Time (Copy #8) | Due: 2026-05-22

Options:
--------------------------------------------------
1. A Game of Thrones                   | by George R. R. Martin [2 copies avail]
--------------------------------------------------
Select a book to borrow: 1

Error: Borrowing limit reached. Basic members can borrow up to 2 books.
```

---

### 14. Borrowing a Book (Failure: Duplicate ISBN)
The system prevents a user from checking out two physical copies of the *exact same book title* simultaneously.

```text
Select Member: 1

Current active borrowings (1):
  - A Game of Thrones (Copy #16) | Due: 2026-06-02

Options:
--------------------------------------------------
1. A Game of Thrones                   | by George R. R. Martin [2 copies avail]
--------------------------------------------------
Select a book to borrow: 1

Error: You already have an active borrowing for 'A Game of Thrones' (ISBN: 978-0553103540).
```

---

### 15. Returning a Book (No Damage & Late Fine)
Demonstrates returning an overdue book where the system calculates a late fee (₹10/day).

```text
--- RETURN A BOOK ---

Options:
--------------------------------------------------
1. Alice Johnson (alice@example.com)
--------------------------------------------------
Select member returning a book: 1

Options:
--------------------------------------------------
1. 1984                      | Copy #1   | Due: 2026-05-15 [OVERDUE]
--------------------------------------------------
Select the book to return: 1

Selected Book: '1984' (Copy #1)

Condition of the book on return:
1. NoDamage
2. MinorDamage
3. DamagedBeyondUsable
4. Lost
Select condition: 1

Return successful!
  Book: 1984 (Copy #1)
  Condition: NoDamage
  Late Fine: ₹30.00 applied for being 3 days overdue.
```

---

### 16. Returning a Book (Damaged Beyond Usable)
A major damage fine is applied, and the book's physical status is updated so it can no longer be borrowed by others.

```text
Select the book to return: 1

Selected Book: 'A Game of Thrones' (Copy #16)

Condition of the book on return:
1. NoDamage
2. MinorDamage
3. DamagedBeyondUsable
4. Lost
Select condition: 3

Return successful!
  Book: A Game of Thrones (Copy #16)
  Condition: DamagedBeyondUsable
  Damage Fine: ₹250.00 applied.
```

---

### 17. Fine Management: View Pending Fines
Shows a detailed breakdown of a member's fines and calculates their total balance using the PostgreSQL stored function.

```text
--- FINE MANAGEMENT ---
1. View Pending Fines
Select: 1

Options:
--------------------------------------------------
1. Alice Johnson (alice@example.com)
--------------------------------------------------
Select Member to view fines: 1

Pending Fines for Alice Johnson:
Fine ID  Type                   Amount     Paid       Created     
-----------------------------------------------------------------
12       LateReturn             ₹30.00    ₹0.00     2026-05-18
13       DamagedBeyondUsable    ₹250.00   ₹0.00     2026-05-18
-----------------------------------------------------------------
Total Pending Balance: ₹280.00
```

---

### 18. Fine Management: Pay Fine (Overpayment & Success)
Demonstrates the system rejecting an overpayment and processing a partial installment payment.

```text
--- FINE MANAGEMENT ---
2. Pay Fine
Select: 2

Options:
--------------------------------------------------
1. Fine #13 | DamagedBeyondUsable | Total: ₹250.00 | Remaining Balance: ₹250.00
--------------------------------------------------
Select fine to pay: 1

Enter payment amount (Remaining balance is ₹250.00): 300.00
Error: Payment (₹300.00) exceeds remaining (₹250.00).

Enter payment amount (Remaining balance is ₹250.00): 100.00

Payment of ₹100.00 recorded.
  Fine #13: ₹100.00/₹250.00 paid.
  Remaining: ₹150.00 | PARTIAL
```

---

### 19. Reporting: Member Borrowing History
Tracking the entire lifecycle of a member's transactions.

```text
--- REPORTS ---
6. Member Borrowing History
Select: 6

Options:
--------------------------------------------------
1. Alice Johnson (alice@example.com)
--------------------------------------------------
Select Member to view history: 1

ID     Book                           Borrow Date    Due Date       Return Date    Status    
-----------------------------------------------------------------------------------------------
11     1984                           2026-05-08    2026-05-15    2026-05-18     Returned  
12     A Brief History of Time        2026-05-10    2026-05-17    Not Returned   Active    
14     A Game of Thrones              2026-05-18    2026-05-25    2026-05-18     Returned  
```

---

### 20. Reporting: Most Borrowed Books
Library analytics showing the most popular items.

```text
--- REPORTS ---
4. Most Borrowed Books
Select: 4

#    ISBN               Title                          Author               Count 
--------------------------------------------------------------------------------
1    978-0451524935     1984                           George Orwell        12    
2    978-0743273565     The Great Gatsby               F. Scott Fitzgerald  8     
3    978-0553103540     A Game of Thrones              George R. R. Martin  5     
4    978-0553380163     A Brief History of Time        Stephen Hawking      2     
```
