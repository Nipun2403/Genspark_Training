using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
      // 1. Build Configuration (Reads from appsettings.json)
      var builder = new ConfigurationBuilder()
          .SetBasePath(AppContext.BaseDirectory)
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
      IConfiguration configuration = builder.Build();

      string connectionString = configuration.GetConnectionString("DefaultConnection")
          ?? throw new InvalidOperationException("Connection string not found.");

      // 2. Setup Dependency Injection Container
      var serviceProvider = new ServiceCollection()
          // Inject Repositories to their Interfaces
          .AddScoped<IUserRepository>(sp => new UserRepository(connectionString))
          .AddScoped<INotificationRepository>(sp => new NotificationRepository(connectionString))

          // Inject Validators and Services
          .AddScoped<UserValidator>()
          .AddScoped<UserService>()
          .AddScoped<NotificationService>()

          // Inject the UI Application
          .AddScoped<ConsoleApplication>()

          .BuildServiceProvider();

      // 3. Resolve the UI application and Run
      var app = serviceProvider.GetRequiredService<ConsoleApplication>();
      await app.RunAsync();
    }
  }
}