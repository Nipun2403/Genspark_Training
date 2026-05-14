using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SharedModels.Interfaces;
using DataAccess;
using BusinessLogic;
using BusinessLogic.Validators;

namespace PresentationUI
{
  class Program
  {
    static async Task Main(string[] args)
    {
      var serviceProvider = new ServiceCollection()
          // 1. Just add the DbContext. It will automatically call OnConfiguring() itself!
          .AddDbContext<AppDbContext>()

          // 2. Standard Repository & Service Injections
          .AddScoped<IUserRepository, UserRepository>()
          .AddScoped<INotificationRepository, NotificationRepository>()
          .AddScoped<UserValidator>()
          .AddScoped<UserService>()
          .AddScoped<NotificationService>()
          .AddScoped<ConsoleApplication>()

          .BuildServiceProvider();

      // 3. Run the App
      var app = serviceProvider.GetRequiredService<ConsoleApplication>();
      await app.RunAsync();
    }
  }
}