# 🏢 3 Tier Notification System v3.0: The Entity Framework Core Edition


## ✨ Fancy Features

* **EF Core Auto-Migrations:** Put away your SQL scripts. The database schema is automatically generated and updated directly from our C# entities.
* **Zero-Bloat Configuration:** We ripped out `appsettings.json` and all the heavy Microsoft Configuration packages. The `DbContext` cleanly manages its own connections via `OnConfiguring`. 
* **Paginated Dashboard (old):** I personally hate a big slop of text. View your sent notifications in a clean, page-by-page UI. It dynamically uses EF Core **Navigation Properties** to `JOIN` tables so you actually know the name of who you are texting.
* **Fail-Fast Regex Validation (old):** Users can never be trusted to type their own phone numbers or emails correctly. Errors are thrown instantly before we ever bother the database.
* **Fancy Visual (old):** Colorful console outputs with green successes, red errors, and magenta headers to make terminal logs look like a kid's birthday cake—tasty and visually appealing.
* **Polymorphic Routing (old):** Dynamically sends Emails or SMS messages using a unified `INotificationSender` interface. Can't be coding if-else all day now can we?
* **Custom Exception Handling (old):** Custom error states (`ValidationException` & `NotFoundException`) because throwing generic `System.Exception` is uncultured for developers.

---
## 🏗️ Folder Structure

The application strictly adheres to the one-layer-does-one-thing rule, aka **Separation of Concerns**. 

```text
EnterpriseNotification/
│
├── EnterpriseNotification.sln         # The Master Binder
│
├── PresentationUI/                    # The View / Controller
│   ├── Program.cs                     # Root File
│   ├── ConsoleApplication.cs          # Fail-fast loops and colorful UI
│   └── PresentationUI.csproj         
│
├── BusinessLogic/                     # The Business Logic Folder
│   ├── NotificationService.cs
│   ├── UserService.cs
│   ├── Validators/
│   │   └── UserValidator.cs           # Regex
│   ├── NotificationSenders/           # Polymorphic implementations
│   │   ├── EmailNotificationSender.cs
│   │   └── SmsNotificationSender.cs
│   └── BusinessLogic.csproj
│
├── DataAccess/                        # The Database Layer (EF Core)
│   ├── AppDbContext.cs                # handles connection & tables
│   ├── NotificationRepository.cs      # LINQ queries
│   ├── UserRepository.cs              
│   └── DataAccess.csproj
│
└── SharedModels/                      # The Entities & Contracts
    ├── User.cs                        # Entity with EF Navigation Properties
    ├── NotificationLog.cs             # Entity with EF Navigation Properties
    ├── NotificationUserJoin.cs        # DTO for the paginated UI
    ├── Interfaces/
    │   ├── IUserRepository.cs
    │   ├── INotificationRepository.cs
    │   └── INotificationSender.cs
    ├── Exceptions/
    │   ├── NotFoundException.cs
    │   └── ValidationException.cs
    └── SharedModels.csproj

```

---

## 🚀 How to Run this Enterprise Beast

### 1. Configure Credentials

Open `DataAccess/AppDbContext.cs` and ensure the connection string inside `OnConfiguring` matches your local PostgreSQL credentials:

```csharp
optionsBuilder.UseNpgsql("Host=localhost;Database=postgres;Username=postgres;Password=");

```

### 2. Build the Database (The EF Core Way)

You don't need to manually create tables in pgAdmin. Open your terminal at the root solution folder and let EF Core do the heavy lifting:

```bash
# Generate the Migration
dotnet ef migrations add InitialCreate --project DataAccess --startup-project PresentationUI

dotnet ef database update --project DataAccess --startup-project PresentationUI
```

### 3. Launch

Once the database is built, run the application:

```bash
dotnet run --project PresentationUI 
```

---

## 📜 My System, My Rules

1. **Add a User:** Provide a name, email, and phone. At least *one* contact method is mandatory. If you skip one, the system will warn you, but let you proceed.
2. **Strict Formats:** Emails must actually look like emails (`user@domain.com`). Phones must be at least 7 digits.
3. **Send a Notification:** Pick a user, pick a type (Email/SMS), and type a message.
4. **The 5-Character Minimum:** Messages under 5 characters will be rejected immediately by the Business Logic.
5. **Pagination:** View your sent notifications 5 at a time. Use `N` for Next, `P` for Previous, and `Q` to Quit back to the menu.

---

## 🖥️ Output Gallery (Application States)

### 1. The Main Dashboard
![alt text](<output/Screenshot 2026-05-11 at 9.54.00 PM.png>)

### 2. Notification Log
![alt text](<output/Screenshot 2026-05-11 at 9.56.35 PM.png>)
