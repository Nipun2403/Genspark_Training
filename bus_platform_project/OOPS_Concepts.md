# Object-Oriented Programming (OOP) Concepts in OmniBus

This document explains how the four fundamental pillars of Object-Oriented Programming (OOP) — **Encapsulation, Inheritance, Abstraction, and Polymorphism** — are implemented and utilized in the OmniBus project.

## 1. Encapsulation

**Concept:** Encapsulation is the mechanism of hiding data (variables) and code acting on the data (methods) together as a single unit. It restricts direct access to some of an object's components, which is a means of preventing accidental interference and misuse.

**Implementation in OmniBus:**
- **Entity Models:** In the `OmniBus.Server/Models` directory, classes like `Bus`, `User`, `Booking`, and `Route` strongly enforce encapsulation by using C# properties with `get` and `set` accessors. 
- **Example:** In `Bus.cs`, properties such as `BusId`, `PlateNumber`, and `Status` are encapsulated. The internal state cannot be arbitrarily modified without going through the defined property constraints (such as `[Required]` or `[MaxLength(20)]` attributes that EF Core validates).
- **Service Configuration:** Database connection strings and JWT keys are encapsulated inside `appsettings.json` and are securely accessed via the `IConfiguration` interface in `Program.cs`, rather than being hardcoded throughout the app.

## 2. Inheritance

**Concept:** Inheritance is a mechanism where a new class is derived from an existing class. It promotes code reusability by allowing the new class to inherit the properties and methods of the base class.

**Implementation in OmniBus:**
- **Database Context:** The most prominent example of inheritance is found in `OmniBus.Server/Data/OmniBusDbContext.cs`.
  ```csharp
  public class OmniBusDbContext : DbContext
  ```
  `OmniBusDbContext` inherits from Entity Framework Core's `DbContext` class. By doing this, our project's context automatically acquires all the complex functionalities required to query, track, and save changes to the PostgreSQL database, without us needing to write that code from scratch.

## 3. Abstraction

**Concept:** Abstraction is the process of hiding the complex implementation details and showing only the essential features of the object. It helps to reduce programming complexity and effort.

**Implementation in OmniBus:**
- **Service Interfaces:** The project uses the Repository/Service pattern extensively. In `Program.cs`, we see registrations like:
  ```csharp
  builder.Services.AddScoped<IAuthService, AuthService>();
  builder.Services.AddScoped<IBusService, BusService>();
  ```
  The interfaces (`IAuthService`, `IBusService`, etc.) define **what** operations can be performed (e.g., `AuthenticateUser`, `GetBuses`), abstracting away **how** they are implemented. The controllers only interact with these interfaces, remaining completely unaware of the underlying database operations, Hangfire jobs, or email sending logic.

## 4. Polymorphism

**Concept:** Polymorphism allows objects to be treated as instances of their parent class rather than their actual class. It allows methods to do different things based on the object it is acting upon (Compile-time via overloading, and Run-time via overriding/interfaces).

**Implementation in OmniBus:**
- **Method Overriding (Run-time Polymorphism):** In `OmniBusDbContext.cs`, the `OnModelCreating` method is overridden:
  ```csharp
  protected override void OnModelCreating(ModelBuilder mb)
  ```
  The base `DbContext` has a virtual `OnModelCreating` method. We override it in our context to provide custom schema definitions, constraints, and data seeding specific to the OmniBus database, demonstrating run-time polymorphism.
- **Dependency Injection (Interface Polymorphism):** Because our controllers rely on interfaces (like `ISeatService`), the DI container in `Program.cs` resolves these to their concrete implementations at runtime. If we were to create a `MockSeatService` for testing, the controller would handle it polymorphically without needing any code changes.

## Summary

The OmniBus project successfully implements all four core OOP concepts natively through its architecture. 
- **Encapsulation** protects data integrity via Models.
- **Inheritance** leverages EF Core's robust `DbContext`.
- **Abstraction** simplifies Controller logic through Service Interfaces.
- **Polymorphism** provides flexibility through Dependency Injection and Method Overriding.

Because these concepts are already well-established in the codebase, the project is highly maintainable, testable, and scalable. No additional fundamental OOP restructuring is necessary, though these principles should be strictly adhered to as new features are developed.
