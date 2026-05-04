using System;
using SimpleNotificationSystem.Models;
using SimpleNotificationSystem.Interface;
using SimpleNotificationSystem.Services;

namespace SimpleNotificationSystem
{
  class Program
  {
    static void Main(string[] args)
    {
      // Sample User
      User myUser = new User
      {
        Name = "Ted Bundy",
        Email = "ted@bundy.com",
        PhoneNumber = "123-456-7890"
      };

      // Email Notification
      INotificationSender emailSender = new EmailNotification();
      NotificationService emailService = new NotificationService(emailSender);
      emailService.NotifyUser(myUser, "This is an email notification!");

      //  SMS Notification 
      INotificationSender smsSender = new SmsNotification();
      NotificationService smsService = new NotificationService(smsSender);
      smsService.NotifyUser(myUser, "This is an SMS notification!");

    }
  }
}
