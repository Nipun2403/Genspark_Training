using System.Text.RegularExpressions;

namespace PresentationLayer.UI;


/// Provides robust, while-loop based input validations and numbered list selections for the console application.

public static class InputValidator
{
    
    /// Gets a non-empty string.
    
    public static string GetString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(input))
                return input;
            
            Console.WriteLine("  [Error] Input cannot be empty. Please try again.");
        }
    }

    
    /// Gets a valid name (letters and spaces only). No numbers allowed.
    
    public static string GetValidName(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("  [Error] Name cannot be empty.");
                continue;
            }

            // Must contain only letters and whitespace (no numbers or special characters)
            if (!Regex.IsMatch(input, @"^[a-zA-Z\s]+$"))
            {
                Console.WriteLine("  [Error] Name cannot contain numbers or special characters. Please try again.");
                continue;
            }

            return input;
        }
    }

    
    /// Gets a valid email address (simple @ and . validation).
    
    public static string GetValidEmail(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("  [Error] Email cannot be empty.");
                continue;
            }

            // Very basic email format regex
            if (!Regex.IsMatch(input, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                Console.WriteLine("  [Error] Invalid email format. Please include an '@' and a domain (e.g., test@example.com).");
                continue;
            }

            return input; // Returning exact case; business logic can normalize it
        }
    }

    
    /// Gets a valid phone number (digits only, length 10-15).
    
    public static string GetValidPhoneNumber(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("  [Error] Phone number cannot be empty.");
                continue;
            }

            if (!Regex.IsMatch(input, @"^\d{10,15}$"))
            {
                Console.WriteLine("  [Error] Invalid phone number. Must contain only digits and be between 10 and 15 digits long.");
                continue;
            }

            return input;
        }
    }

    
    /// Gets a valid integer input greater than or equal to a minimum value.
    
    public static int GetValidInt(string prompt, int min = 0)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine()?.Trim(), out int result) && result >= min)
            {
                return result;
            }
            Console.WriteLine($"  [Error] Please enter a valid whole number (minimum: {min}).");
        }
    }
    
    
    /// Gets a valid decimal input greater than zero.
    
    public static decimal GetValidDecimal(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (decimal.TryParse(Console.ReadLine()?.Trim(), out decimal result) && result > 0)
            {
                return result;
            }
            Console.WriteLine("  [Error] Please enter a valid decimal number greater than 0.");
        }
    }

    
    /// Renders a numbered list and forces the user to select a valid index.
    /// Returns the strongly-typed object from the list.
    
    public static T? GetSelection<T>(List<T> items, string prompt, Func<T, string> displaySelector) where T : class
    {
        if (items == null || items.Count == 0)
        {
            throw new ArgumentException("The selection list cannot be empty.");
        }

        Console.WriteLine("\nOptions:");
        Console.WriteLine(new string('-', 50));
        for (int i = 0; i < items.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {displaySelector(items[i])}");
        }
        Console.WriteLine(new string('-', 50));

        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();
            
            // Allow cancellation by typing 0
            if (input == "0") 
                return default; 

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= items.Count)
            {
                return items[choice - 1];
            }
            
            Console.WriteLine($"  [Error] Invalid choice. Please enter a number between 1 and {items.Count}. (Type 0 to cancel)");
        }
    }
}
