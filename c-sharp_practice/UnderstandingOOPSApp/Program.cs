using System;
using UnderstandingOOPSApp.Interfaces;
using UnderstandingOOPSApp.Services;

namespace UnderstandingOOPSApp
{
  internal class Program
  {
    ICustomerInteract customerInteract;
    public Program()
    {
      customerInteract = new CustomerService();
    }
    void DoBanking()
    {
      bool isRunning = true;
      while (isRunning)
      {
        Console.WriteLine("1. Add account");
        Console.WriteLine("2. Print Account details giving account number");
        Console.WriteLine("3. Print account details using phone number");
        Console.WriteLine("4. Exit");

        string choice = Console.ReadLine() ?? "";

        switch (choice)
        {
          case "1":
            var account = customerInteract.OpensAccount();
            Console.WriteLine(account);
            break;
          case "2":
            Console.WriteLine("Please enter the account you like see");
            string accNum = Console.ReadLine() ?? "";
            customerInteract.PrintAccountDetails(accNum);
            break;
          case "3":
            Console.WriteLine("Please enter the phone number you like see");
            string phoneNum = Console.ReadLine() ?? "";
            customerInteract.PrintAccountDetailsByPhone(phoneNum);
            break;
          case "4":
            isRunning = false;
            break;
          default:
            Console.WriteLine("Invalid choice");
            break;
        }
      }

    }
    static void Main(string[] args)
    {
      new Program().DoBanking();
    }
  }
}