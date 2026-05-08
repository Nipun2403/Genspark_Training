using System;
using System.Collections.Generic;
using System.Linq;
using SharedModels;
using SharedModels.Exceptions;
using BusinessLogic;
using BusinessLogic.Validators;

// -------> WARNING : A very long file, pls have patience when reviewing :P

//  Also Used a lot of console colors learnt from the last project to give it some jazz and character. Nobody likes bland interfaces anyways :P
namespace PresentationUI
{
  public class ConsoleApplication
  {
    private readonly UserService _userService;
    private readonly NotificationService _notificationService;
    private readonly UserValidator _userValidator;

    // Using Validator in UI layer to not call service layer for a bad input over and over.
    // Also helps to call out the error fast in UI layer only.
    public ConsoleApplication(UserService userService, NotificationService notificationService, UserValidator userValidator)
    {
      _userService = userService;
      _notificationService = notificationService;
      _userValidator = userValidator;
    }

    public void Run()
    {
      SeedInitialData();
      bool isRunning = true;

      while (isRunning)
      {
        Console.Clear();
        RenderHeader();
        RenderMenu();

        string choice = Console.ReadLine() ?? string.Empty;

        try
        {
          isRunning = ProcessUserChoice(choice);
        }
        catch (ValidationException ex) { PrintError($"\n[Business Rule Violation] {ex.Message}"); Pause(); }
        catch (NotFoundException ex) { PrintError($"\n[Data Error] {ex.Message}"); Pause(); }
        catch (Exception ex) { PrintError($"\n[System Error] {ex.Message}"); Pause(); }
      }
    }

    //  Random Data used from previous project to have something to work with. 
    //  I'd be irritated if there was empty user list and i have to create user before doing anything else :/
    private void SeedInitialData()
    {
      _userService.CreateUser("Bunty", "bunty@example.com", "123-456-7890");
      _userService.CreateUser("Raju", "raju@gmail.com", "987-654-3210");
    }

    private void RenderHeader()
    {
      Console.ForegroundColor = ConsoleColor.DarkCyan;
      Console.WriteLine("=================================================");
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("        3 TIER NOTIFICATION SYSTEM");
      Console.ForegroundColor = ConsoleColor.DarkCyan;
      Console.WriteLine("=================================================\n");
      Console.ResetColor();
    }

    private void RenderMenu()
    {
      // Main Menu
      Console.WriteLine("  1. Add new User");
      Console.WriteLine("  2. View All Users");
      Console.WriteLine("  3. View User by ID");
      Console.WriteLine("  4. Update User");
      Console.WriteLine("  5. Delete User");
      Console.WriteLine("  6. Send Notification");
      Console.WriteLine("  7. View Sent Notifications");
      Console.WriteLine("  8. Exit");
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.Write("\n  Choose an option >> ");
      Console.ResetColor();
    }

    private bool ProcessUserChoice(string choice)
    {
      Console.WriteLine();
      switch (choice)
      {
        case "1": AddUserWorkflow(); Pause(); return true;
        case "2": ViewAllUsersWorkflow(); Pause(); return true;
        case "3": ViewByIdWorkflow(); Pause(); return true;
        case "4": UpdateUserWorkflow(); Pause(); return true;
        case "5": DeleteUserWorkflow(); Pause(); return true;
        case "6": SendNotificationWorkflow(); Pause(); return true;
        case "7": ViewNotificationsWorkflow(); return true;
        case "8":
          PrintInfo("Shutting down system. Goodbye!");
          return false;
        default:
          PrintError("Invalid option. Please select 1-8.");
          Pause();
          return true;
      }
    }

    private void AddUserWorkflow()
    {
      PrintTitle("--- Add New User ---");

      string name;
      while (true)
      {
        Console.Write("Enter Name: ");
        name = Console.ReadLine() ?? string.Empty;
        // validating name at the input field to avoid useless calls to service layer
        try { _userValidator.ValidateName(name); break; }
        catch (ValidationException ex) { PrintError($"  -> {ex.Message}"); }
      }

      string email;
      while (true)
      {
        Console.Write("Enter Email (Press Enter to skip): ");
        email = Console.ReadLine() ?? string.Empty;
        try { _userValidator.ValidateEmail(email); break; }
        catch (ValidationException ex) { PrintError($"  -> {ex.Message}"); }
      }


      //  If email is not entered, makes it a trap for user to enter phone number. Can't have a user without any contanct info.
      // Can't contanct a ghost user after all :O
      string phone = string.Empty;
      if (string.IsNullOrWhiteSpace(email))
      {
        PrintError("  -> No email entered. Phone number is mandatory.");
        while (true)
        {
          Console.Write("Enter Phone Number: ");
          phone = Console.ReadLine() ?? string.Empty;
          if (string.IsNullOrWhiteSpace(phone))
          {
            PrintError("  -> Phone number is mandatory since email is blank.");
            continue;
          }
          try { _userValidator.ValidatePhone(phone); break; }
          catch (ValidationException ex) { PrintError($"  -> {ex.Message}"); }
        }
      }
      else
      {
        while (true)
        {
          Console.Write("Enter Phone Number (Press Enter to skip): ");
          phone = Console.ReadLine() ?? string.Empty;

          // Since email is provided, it gives a soft warning to user that they can override or skip if they are really snarky about privacy or something idk.
          if (string.IsNullOrWhiteSpace(phone))
          {
            PrintWarning("  -> Advisable to enter phone number for better communication.");
            Console.Write("     Press Enter again to confirm skip, or type number: ");
            string confirm = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(confirm)) break;
            phone = confirm;
          }

          try { _userValidator.ValidatePhone(phone); break; }
          catch (ValidationException ex) { PrintError($"  -> {ex.Message}"); }
        }
      }

      var newUser = _userService.CreateUser(name, email, phone);
      PrintSuccess($"\nUser created successfully! Assigned ID: {newUser.Id}");
    }

    private void UpdateUserWorkflow()
    {
      PrintTitle("--- Update User ---");
      Console.Write("Enter User ID to update: ");
      if (!int.TryParse(Console.ReadLine(), out int updateId)) return;

      var existingUser = _userService.GetUserById(updateId);
      PrintInfo($"Updating: {existingUser.Name} (Email: {existingUser.Email} | Phone: {existingUser.PhoneNumber})");

      string nName;
      while (true)
      {
        Console.Write("Enter new Name (leave blank to keep): ");
        nName = Console.ReadLine() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nName)) break;
        try { _userValidator.ValidateName(nName); break; }
        catch (ValidationException ex) { PrintError($"  -> {ex.Message}"); }
      }

      string nEmail;
      while (true)
      {
        Console.Write("Enter new Email (leave blank to keep): ");
        nEmail = Console.ReadLine() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nEmail)) break;
        try { _userValidator.ValidateEmail(nEmail); break; }
        catch (ValidationException ex) { PrintError($"  -> {ex.Message}"); }
      }

      string nPhone;
      while (true)
      {
        Console.Write("Enter new Phone (leave blank to keep): ");
        nPhone = Console.ReadLine() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nPhone)) break;
        try { _userValidator.ValidatePhone(nPhone); break; }
        catch (ValidationException ex) { PrintError($"  -> {ex.Message}"); }
      }

      _userService.UpdateUser(updateId, nName, nEmail, nPhone);
      PrintSuccess($"\nUser with ID : {updateId} Updated.");
    }

    private void DeleteUserWorkflow()
    {
      PrintTitle("--- Delete User ---");
      Console.Write("Enter User ID to delete: ");
      if (!int.TryParse(Console.ReadLine(), out int deleteId)) return;

      _userService.DeleteUser(deleteId);
      PrintSuccess($"User with ID : {deleteId} Deleted.");
    }

    private void ViewNotificationsWorkflow()
    {
      var logs = _notificationService.GetSortedSentNotifications();
      if (!logs.Any())
      {
        PrintWarning("No notifications sent yet.");
        Pause();
        return;
      }

      int pageSize = 5;
      int totalPages = (int)Math.Ceiling(logs.Count / (double)pageSize);
      int currentPage = 1;


      // Very fancy notification log with colors, page and all.
      // Took 2 redbulls to make it work. redbulls > coffee . Yes i said it.
      while (true)
      {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("=================================================");
        Console.WriteLine($"         NOTIFICATION LOG (Page {currentPage}/{totalPages})");
        Console.WriteLine("=================================================\n");
        Console.ResetColor();

        var pageItems = logs.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

        foreach (var log in pageItems)
        {
          Console.ForegroundColor = ConsoleColor.DarkGray;
          Console.WriteLine("-------------------------------------------------");
          Console.ResetColor();

          Console.Write("- To user Id : ");
          Console.ForegroundColor = ConsoleColor.Yellow;
          Console.WriteLine(log.UserId);
          Console.ResetColor();

          Console.WriteLine($"- Timestamp  : {log.NotificationPayload.SentAt}");
          Console.WriteLine($"- Type       : {log.NotificationType}");

          Console.Write("- Content    : ");
          Console.ForegroundColor = ConsoleColor.White;
          Console.WriteLine(log.NotificationPayload.Message);
          Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("-------------------------------------------------\n");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[N]ext Page | [P]revious Page | [Q]uit to Menu");
        Console.Write(">> ");
        Console.ResetColor();

        string input = Console.ReadLine()?.ToUpper() ?? "";
        if (input == "Q") break;
        if (input == "N" && currentPage < totalPages) currentPage++;
        else if (input == "P" && currentPage > 1) currentPage--;
        else if (input != "N" && input != "P") PrintError("Invalid command.");
      }
    }

    // View all user with a nice header and format
    private void ViewAllUsersWorkflow()
    {
      PrintTitle("--- Registered Users ---");
      var users = _userService.GetAllUsers();
      foreach (var u in users)
      {
        Console.WriteLine($"ID: {u.Id} | Name: {u.Name} | Email: {u.Email} | Phone: {u.PhoneNumber}");
      }
    }

    private void ViewByIdWorkflow()
    {
      Console.Write("Enter ID: ");
      if (int.TryParse(Console.ReadLine(), out int id))
      {
        var user = _userService.GetUserById(id);
        PrintSuccess($"Found: {user.Name} (Email: {user.Email} | Phone: {user.PhoneNumber})");
      }
    }

    // This is shows when sending a notification.
    private void SendNotificationWorkflow()
    {
      PrintTitle("--- Dispatch Notification ---");
      Console.Write("Enter Target User ID: ");
      if (!int.TryParse(Console.ReadLine(), out int targetId)) return;

      Console.Write("Enter Notification Type (Email/SMS): ");
      string type = Console.ReadLine() ?? string.Empty;

      Console.Write("Enter Message: ");
      string msg = Console.ReadLine() ?? string.Empty;

      _notificationService.SendNotification(targetId, type, msg);
      PrintSuccess("\nNotification Dispatched Successfully!");
    }



    // Console Color Functios to avoid typing all of this again and again
    // yes, I'm very lazy :P
    private void PrintTitle(string msg)
    {
      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.WriteLine($"\n{msg}\n");
      Console.ResetColor();
    }

    private void PrintSuccess(string msg)
    {
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine(msg);
      Console.ResetColor();
    }

    private void PrintError(string msg)
    {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine(msg);
      Console.ResetColor();
    }

    private void PrintWarning(string msg)
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine(msg);
      Console.ResetColor();
    }

    private void PrintInfo(string msg)
    {
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine(msg);
      Console.ResetColor();
    }

    private void Pause()
    {
      Console.ForegroundColor = ConsoleColor.DarkGray;
      Console.Write("\nPress Enter to continue...");
      Console.ReadLine();
      Console.ResetColor();
    }
  }
}