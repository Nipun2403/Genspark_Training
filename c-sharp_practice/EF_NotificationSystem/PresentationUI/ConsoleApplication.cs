using System;
using System.Linq;
using System.Threading.Tasks;
using SharedModels.Exceptions;
using BusinessLogic;
using BusinessLogic.Validators;

namespace PresentationUI
{
  public class ConsoleApplication
  {
    private readonly UserService _userService;
    private readonly NotificationService _notificationService;
    private readonly UserValidator _userValidator;

    public ConsoleApplication(UserService userService, NotificationService notificationService, UserValidator userValidator)
    {
      _userService = userService;
      _notificationService = notificationService;
      _userValidator = userValidator;
    }

    public async Task RunAsync()
    {
      bool isRunning = true;
      while (isRunning)
      {
        Console.Clear();
        RenderMenu();
        string choice = Console.ReadLine() ?? string.Empty;

        try { isRunning = await ProcessUserChoiceAsync(choice); }
        catch (ValidationException ex) { PrintError($"\n[Rule Violation] {ex.Message}"); Pause(); }
        catch (NotFoundException ex) { PrintError($"\n[Data Error] {ex.Message}"); Pause(); }
        catch (Exception ex) { PrintError($"\n[System Error] {ex.Message}"); Pause(); }
      }
    }

    private void RenderMenu()
    {
      Console.ForegroundColor = ConsoleColor.DarkCyan;
      Console.WriteLine("=================================================");
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("        FANCY NOTIFICATION SYSTEM");
      Console.ForegroundColor = ConsoleColor.DarkCyan;
      Console.WriteLine("=================================================\n");
      Console.ResetColor();
      Console.WriteLine("  1. Add new User\n  2. View All Users\n  3. View User by ID\n  4. Update User\n  5. Delete User\n  6. Send Notification\n  7. View Sent Notifications\n  8. Exit");
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.Write("\n  Choose an option >> ");
      Console.ResetColor();
    }

    private async Task<bool> ProcessUserChoiceAsync(string choice)
    {
      Console.WriteLine();
      switch (choice)
      {
        case "1": await AddUserWorkflowAsync(); Pause(); return true;
        case "2": await ViewAllUsersWorkflowAsync(); Pause(); return true;
        case "3": await ViewByIdWorkflowAsync(); Pause(); return true;
        case "4": await UpdateUserWorkflowAsync(); Pause(); return true;
        case "5": await DeleteUserWorkflowAsync(); Pause(); return true;
        case "6": await SendNotificationWorkflowAsync(); Pause(); return true;
        case "7": await ViewNotificationsWorkflowAsync(); return true;
        case "8": PrintInfo("Shutting down system. Goodbye!"); return false;
        default: PrintError("Invalid option."); Pause(); return true;
      }
    }

    private async Task AddUserWorkflowAsync()
    {
      Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine("--- Add New User ---\n"); Console.ResetColor();

      string name;
      while (true) { Console.Write("Enter Name: "); name = Console.ReadLine() ?? ""; try { _userValidator.ValidateName(name); break; } catch (ValidationException ex) { PrintError($"  -> {ex.Message}"); } }

      string email;
      while (true) { Console.Write("Enter Email (Press Enter to skip): "); email = Console.ReadLine() ?? ""; try { _userValidator.ValidateEmail(email); break; } catch (ValidationException ex) { PrintError($"  -> {ex.Message}"); } }

      string phone = string.Empty;
      if (string.IsNullOrWhiteSpace(email))
      {
        PrintError("  -> No email entered. Phone number is mandatory.");
        while (true) { Console.Write("Enter Phone Number: "); phone = Console.ReadLine() ?? ""; if (string.IsNullOrWhiteSpace(phone)) continue; try { _userValidator.ValidatePhone(phone); break; } catch (ValidationException ex) { PrintError($"  -> {ex.Message}"); } }
      }
      else
      {
        while (true) { Console.Write("Enter Phone (Press Enter to skip): "); phone = Console.ReadLine() ?? ""; if (string.IsNullOrWhiteSpace(phone)) break; try { _userValidator.ValidatePhone(phone); break; } catch (ValidationException ex) { PrintError($"  -> {ex.Message}"); } }
      }

      var newUser = await _userService.CreateUserAsync(name, email, phone);
      PrintSuccess($"\nUser created successfully! Assigned ID: {newUser.Id}");
    }

    private async Task UpdateUserWorkflowAsync()
    {
      Console.Write("Enter User ID to update: "); if (!int.TryParse(Console.ReadLine(), out int updateId)) return;
      var existingUser = await _userService.GetUserByIdAsync(updateId);

      Console.Write("Enter new Name (leave blank to keep): "); string nName = Console.ReadLine() ?? "";
      Console.Write("Enter new Email (leave blank to keep): "); string nEmail = Console.ReadLine() ?? "";
      Console.Write("Enter new Phone (leave blank to keep): "); string nPhone = Console.ReadLine() ?? "";

      await _userService.UpdateUserAsync(updateId, nName, nEmail, nPhone);
      PrintSuccess($"\nUser with ID : {updateId} Updated.");
    }

    private async Task DeleteUserWorkflowAsync()
    {
      Console.Write("Enter User ID to delete: "); if (!int.TryParse(Console.ReadLine(), out int deleteId)) return;
      await _userService.DeleteUserAsync(deleteId);
      PrintSuccess($"User with ID : {deleteId} Deleted.");
    }

    private async Task ViewAllUsersWorkflowAsync()
    {
      var users = await _userService.GetAllUsersAsync();
      foreach (var u in users) Console.WriteLine($"ID: {u.Id} | Name: {u.Name} | Email: {u.Email} | Phone: {u.PhoneNumber}");
    }

    private async Task ViewByIdWorkflowAsync()
    {
      Console.Write("Enter ID: "); if (int.TryParse(Console.ReadLine(), out int id)) { var user = await _userService.GetUserByIdAsync(id); PrintSuccess($"Found: {user.Name} (Email: {user.Email} | Phone: {user.PhoneNumber})"); }
    }

    private async Task SendNotificationWorkflowAsync()
    {
      Console.Write("Enter Target User ID: "); if (!int.TryParse(Console.ReadLine(), out int targetId)) return;
      Console.Write("Enter Notification Type (Email/SMS): "); string type = Console.ReadLine() ?? "";
      Console.Write("Enter Message: "); string msg = Console.ReadLine() ?? "";
      await _notificationService.ProcessAndSendNotificationAsync(targetId, type, msg);
      PrintSuccess("\nNotification Dispatched Successfully!");
    }

    private async Task ViewNotificationsWorkflowAsync()
    {
      var logs = await _notificationService.GetSortedSentNotificationsAsync();
      if (!logs.Any()) { PrintWarning("No notifications sent yet."); Pause(); return; }

      int pageSize = 5; int totalPages = (int)Math.Ceiling(logs.Count / (double)pageSize); int currentPage = 1;

      while (true)
      {
        Console.Clear(); Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("================================================="); Console.WriteLine($"         NOTIFICATION LOG (Page {currentPage}/{totalPages})"); Console.WriteLine("=================================================\n"); Console.ResetColor();

        var pageItems = logs.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        foreach (var log in pageItems)
        {
          Console.ForegroundColor = ConsoleColor.DarkGray; Console.WriteLine("-------------------------------------------------"); Console.ResetColor();
          Console.Write("- To         : "); Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine($"{log.UserName} ({log.UserEmail})"); Console.ResetColor();
          Console.WriteLine($"- Timestamp  : {log.SentAt}\n- Type       : {log.NotificationType}");
          Console.Write("- Content    : "); Console.ForegroundColor = ConsoleColor.White; Console.WriteLine(log.Message); Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.DarkGray; Console.WriteLine("-------------------------------------------------\n"); Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[N]ext Page | [P]revious Page | [Q]uit to Menu"); Console.Write(">> "); Console.ResetColor();

        string input = Console.ReadLine()?.ToUpper() ?? "";
        if (input == "Q") break;
        if (input == "N" && currentPage < totalPages) currentPage++; else if (input == "P" && currentPage > 1) currentPage--;
      }
    }

    private void PrintSuccess(string msg) { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine(msg); Console.ResetColor(); }
    private void PrintError(string msg) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine(msg); Console.ResetColor(); }
    private void PrintWarning(string msg) { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine(msg); Console.ResetColor(); }
    private void PrintInfo(string msg) { Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine(msg); Console.ResetColor(); }
    private void Pause() { Console.ForegroundColor = ConsoleColor.DarkGray; Console.Write("\nPress Enter to continue..."); Console.ReadLine(); Console.ResetColor(); }
  }
}