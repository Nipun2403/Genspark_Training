// Console.WriteLine("Hello, World!");

using System;
using System.Collections.Generic;
using SimpleNotificationSystem_V2_CRUD.Models;
using SimpleNotificationSystem_V2_CRUD.Services;
using SimpleNotificationSystem_V2_CRUD.Interface;

namespace SimpleNotificationSystem_V2_CRUD
{
  class Program
  {
    static void Main(string[] args)
    {

      UserService userService = new UserService();
      bool isRunning = true;

      userService.CreateUser("Bunty", "bunty@example.com", "123-456-7890");
      userService.CreateUser("Raju", "raju@gmail.com", "987-654-3210");

      //  Loop wil. run until user chooses to exit
      while (isRunning)
      {
        // New options added to the menu for CURD (Class Joke Reference) operations on User
        Console.WriteLine("\n ----Menu----\n");
        Console.WriteLine("1. Add new User");
        Console.WriteLine("2. View All User");
        Console.WriteLine("3. View User by ID");
        Console.WriteLine("4. Update User");
        Console.WriteLine("5. Delete User");
        Console.WriteLine("6. Exit");
        Console.Write("\nChoose an option: ");


        string choice = Console.ReadLine() ?? string.Empty;

        switch (choice)
        {
          //  Crete new User
          case "1":
            Console.Write("\n ----Create New User----\n");
            Console.Write("Enter Name: ");
            string name = Console.ReadLine() ?? string.Empty;
            Console.Write("Enter Email: ");
            string email = Console.ReadLine() ?? string.Empty;
            Console.Write("Enter Phone Number: ");
            string phoneNumber = Console.ReadLine() ?? string.Empty;

            User newUser = userService.CreateUser(name, email, phoneNumber);
            Console.WriteLine($"User created with ID: {newUser.Id}");
            break;

          // View all Users
          case "2":
            Console.Write("\n----All Users----\n");
            List<User> users = userService.GetAllUsers();
            foreach (var user in users)
            {
              Console.WriteLine($"ID: {user.Id}, Name: {user.Name}, Email: {user.Email}, Phone: {user.PhoneNumber}");
            }
            break;

          //  Search User by ID
          case "3":
            Console.Write("\n ----Search User----\n");
            Console.Write("Enter ID: ");
            int id = int.Parse(Console.ReadLine() ?? "0");
            User? userFind = userService.GetUserById(id);
            if (userFind != null)
            {
              Console.WriteLine($"User found: ID: {userFind.Id}, Name: {userFind.Name}, Email: {userFind.Email}, Phone: {userFind.PhoneNumber}");
            }
            else
            {
              Console.WriteLine("User not found.");
            }
            break;

          // Update User details
          case "4":
            Console.Write("\n ----Update User----\n");
            Console.Write("Enter User ID to update: ");
            int updateId = int.Parse(Console.ReadLine() ?? "0");

            // Check if the user exists before asking for new details, using case 3 logic
            User? userToUpdate = userService.GetUserById(updateId);
            if (userToUpdate == null)
            {
              Console.WriteLine("User not found.");
              break;
            }

            Console.Write("Enter new Name (leave blank to keep unchanged): ");
            string newName = Console.ReadLine() ?? string.Empty;
            Console.Write("Enter new Email (leave blank to keep unchanged): ");
            string newEmail = Console.ReadLine() ?? string.Empty;
            Console.Write("Enter new Phone Number (leave blank to keep unchanged): ");
            string newPhoneNumber = Console.ReadLine() ?? string.Empty;

            bool isUpdated = userService.UpdateUser(updateId, newName, newEmail, newPhoneNumber);
            if (isUpdated)
              Console.WriteLine($"User {updateId} updated successfully.");
            else
              Console.WriteLine("User not found.");
            break;

          // Delete User
          case "5":
            Console.Write("\n ----Delete User----\n");
            Console.Write("Enter User ID to delete: ");
            int deleteId = int.Parse(Console.ReadLine() ?? "0");
            bool isDeleted = userService.DeleteUser(deleteId);
            if (isDeleted)
              Console.WriteLine($"User {deleteId} deleted successfully.");
            else
              Console.WriteLine("User not found.");
            break;

          // Exit the application
          case "6":
            Console.Write("\n ----Goodbye----\n");
            isRunning = false;
            break;

          // If anything else is entered, show an error message
          default:
            Console.WriteLine("Invalid option. Please try again.");
            break;
        }

      }
    }
  }
}