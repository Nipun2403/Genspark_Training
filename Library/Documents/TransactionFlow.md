# Transaction Flow

## App FLow :

```
[PresentationLayer] (Console App)
       │
       ▼
[BusinessLogicLayer] (Class Library)
       │
       ▼
[DataAccessLayer] (Class Library) ──► PostgreSQL
```

## Transaction :
```mermaid
flowchart TD
    A[Start Transaction] --> B{Member Active?}
    B -- No --> FAIL[Rollback & Return Error]
    B -- Yes --> C{Unpaid Fines ≤ ₹500?}
    C -- No --> FAIL
    C -- Yes --> D{Active Borrowings < Limit?}
    D -- No --> FAIL
    D -- Yes --> E{Book Copy Available?}
    E -- No --> FAIL
    E -- Yes --> F{No Duplicate Active Borrowing?}
    F -- No --> FAIL
    F -- Yes --> G[Create Borrowing Record]
    G --> H[Update BookCopy Status → Borrowed]
    H --> I[Commit Transaction]
```
