# 🏢 3 Tier Notification System v2.0: The Database Edition

It is built using strictly enforced **3-Tier Architecture** and **SOLID Principles**, including Dependency Injection, multi-project solution structure, Interface-driven polymorphism, and now—a fully relational **PostgreSQL Database** powered by raw ADO.NET. Because saving data in temporary in-memory lists is for *interns*.

---

## ✨ Fancy Features

* **PostgreSQL** Tata bye bye to losing all your data every time the app restarts. Data is safe in relational tables.
* **Asynchronous (Async/Await):** Because blocking the main thread during a database call is a crime against my session requests.
* **Dependency Injection (IoC):** Completely decoupled architecture using `Microsoft.Extensions.DependencyInjection`. (took some help of ai in this)
* **Paginated Dashboard (same as last time):** I personally hate a big slop of text. View your sent notifications in a clean, page-by-page UI. It even dynamically uses SQL `JOIN`s so you actually know the name of who you are texting.
* **Fail-Fast Regex Validation (same as last time):** Users can never be trusted to type their own phone numbers or emails correctly. Errors are thrown instantly before we ever bother the database.
* **Fancy Visual (same as last time):** Colorful console outputs with green successes, red errors, and magenta headers to make terminal logs look like a kid's birthday cake—tasty and visually appealing.
* **Polymorphic Routing (same as last time):** Dynamically sends Emails or SMS messages using a unified `INotificationSender` interface. Can't be coding if-else all day now can we?
* **Custom Exception Handling:** Custom error states (`ValidationException` & `NotFoundException`) because throwing generic `System.Exception` is uncultured for developers.

---

## 🏗️ Folder Structure

The application strictly adheres to the one-layer-does-one-thing rule, aka **Separation of Concerns**. Interfaces dictate the contracts, and implementations do the heavy lifting.

```text
Fancy Notification/
│
├── notification.sln         # The Master Binder
│
├── PresentationUI/                    # The View / Controller
│   ├── Program.cs                     # Composition Root (DI Setup & AppContext)
│   ├── ConsoleApplication.cs          # colorful UI
│   ├── appsettings.json               # DB Credentials (DO NOT COMMIT TO GIT)
│   └── PresentationUI.csproj
│
├── BusinessLogic/                     # The Brains (Rules & Validation)
│   ├── NotificationService.cs
│   ├── UserService.cs
│   ├── Validators/
│   │   └── UserValidator.cs           # Regex central
│   ├── NotificationSenders/           # Polymorphic implementations
│   │   ├── EmailNotificationSender.cs
│   │   └── SmsNotificationSender.cs
│   └── BusinessLogic.csproj
│
├── DataAccess/                        # The Database Layer (ADO.NET & Npgsql)
│   ├── NotificationRepository.cs      # Executes raw Postgres INSERTs/JOINs
│   ├── UserRepository.cs              
│   └── DataAccess.csproj
│
└── SharedModels/                      # The Entities & Contracts
    ├── User.cs
    ├── NotificationLog.cs             # Flattened DB Entity
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
## Output
#### 1. Main Menu
![output1](<output/Screenshot 2026-05-11 at 9.54.00 PM.png>)

#### 2. Notification Logs
![alt text](<output/Screenshot 2026-05-11 at 9.56.35 PM.png>)