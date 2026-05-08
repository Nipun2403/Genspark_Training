# 🏢 3 Tier Notification System : 

It is built using strictly enforced **3-Tier Architecture** and **SOLID Principles**, including Dependency Injection, multi-project solution structure, and Interface-driven polymorphism.  A

---

## ✨ Fancy Features

* **Paginated Dashboard (Extra):** I personally hates a big slop of text. So View your sent notifications in a clean UI
* **Fail-Fast Regex Validation (Bonus):** users can never be trusted to type their own phone numbers or emails correctly. Errors are thrown instantly.
* **Fancy Visual:** Colorful console outputs with green successes, red errors, and magenta headers to make terminal logs look like a kid's birthday cake, tasty and visually appealing
* **Polymorphic Routing:** Dynamically sends Emails or SMS messages using a unified `INotificationSender` interface. Can't be coding if-else all day now can we?
* **Custom Exception Handling:** Custom error states (`ValidationException` & `NotFoundException`) because throwing generic `System.Exception` is uncultured for developers.

---

## 🏗️ Folder Structure

The application strictly adheres to the one-layer-does-one-thing rule, aka **Separation of Concerns**.

```text
EnterpriseNotification/
│
├── EnterpriseNotification.sln         # The Master Binder
│
├── PresentationUI/                    # The View / Controller
│   ├── Program.cs                     # Composition Root (Startup)
│   ├── ConsoleApplication.cs          # Fail-fast loops and colorful UI
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
├── DataAccess/                        # The Database (In-Memory Lists)
│   ├── NotificationRepository.cs
│   ├── UserRepository.cs
│   └── DataAccess.csproj
│
└── SharedModels/                      # The Entities (Data Transfer Objects)
    ├── User.cs
    ├── Notification.cs
    ├── NotificationLog.cs
    ├── Interfaces/
    │   └── INotificationSender.cs
    ├── Exceptions/
    │   ├── NotFoundException.cs
    │   └── ValidationException.cs
    └── SharedModels.csproj

```

---

## If you want to run this

1. Clone the repository to your local machine.
2. Navigate to the root directory containing the `.sln` file.
3. Run the command specifying the UI project:
```bash
dotnet run --project PresentationUI 

```



---

## 📜 My System

1. **Add a User:** Provide a name, email, and phone. At least *one* contact method is mandatory. If you skip one, the system will warn you, but let you proceed.
2. **Strict Formats:** Emails must actually look like emails (`user@domain.com`). Phones must be at least 7 digits.
3. **Send a Notification:** Pick a user, pick a type (Email/SMS), and type a message.
4. **The 5-Character Minimum:** Messages under 5 characters will be rejected immediately.
5. **Pagination:** View your sent notifications 5 at a time. Use `N` for Next, `P` for Previous, and `Q` to Quit back to the menu.

---

## 🖥️ Output Gallery (Application States)

### 1. The Main Dashboard
![alt text](<output/Screenshot 2026-05-08 at 9.57.54 PM.png>)
![alt text](<output/Screenshot 2026-05-08 at 9.58.23 PM.png>)
![alt text](<output/Screenshot 2026-05-08 at 9.59.24 PM.png>)
### 2. Paginated Notification Viewer
![alt text](<output/Screenshot 2026-05-08 at 10.00.07 PM.png>)