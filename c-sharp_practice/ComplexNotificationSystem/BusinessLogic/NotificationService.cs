using System;
using System.Collections.Generic;
using System.Linq;
using SharedModels;
using SharedModels.Exceptions; // Random Custom excpetion handling to make sure i cover all the assignment pointers.
using SharedModels.Interfaces;
using DataAccess;
using BusinessLogic.NotificationSenders;

namespace BusinessLogic
{
  public class NotificationService
  {
    private readonly NotificationRepository _repository;
    private readonly UserService _userService;

    public NotificationService(NotificationRepository repository, UserService userService)
    {
      _repository = repository;
      _userService = userService;
    }

    public void SendNotification(int userId, string notificationType, string messageText)
    {
      // Business Logic Validation as per the assignment requirements
      if (string.IsNullOrWhiteSpace(messageText)) // empty or white space check
        throw new ValidationException("Message cannot be empty.");
      if (messageText.Length < 5) // min length check
        throw new ValidationException("Message length must be at least 5 characters.");

      User user = _userService.GetUserById(userId);
      Notification notification = new Notification(messageText, DateTime.Now);

      INotificationSender sender;

      if (notificationType.ToUpper() == "EMAIL")
      {
        // Email check to ensure @ is present, very minimal validation as a light backup check in case something break or i miss something.
        // Already used regex validatio of email when creating user to enusre stroger validation there. 
        if (string.IsNullOrWhiteSpace(user.Email) || !user.Email.Contains("@"))
          throw new ValidationException("User does not have a valid email address.");
        sender = new EmailNotificationSender();
      }
      else if (notificationType.ToUpper() == "SMS")
      {
        if (string.IsNullOrWhiteSpace(user.PhoneNumber))
          throw new ValidationException("User does not have a valid phone number.");
        if (messageText.Length > 160) // Assumed <160 does not include 160th character. 
          throw new ValidationException("SMS message cannot exceed 160 characters.");
        sender = new SmsNotificationSender();
      }
      else
      {
        throw new ValidationException("Invalid notification type. Choose 'Email' or 'SMS'.");
      }

      // Polymorphic execution as asked in assignment
      sender.SendNotification(user, notification);

      // Audit logging
      var log = new NotificationLog
      {
        UserId = user.Id,
        NotificationType = notificationType.ToUpper(),
        NotificationPayload = notification
      };
      _repository.LogNotification(log);
    }


    public List<NotificationLog> GetSortedSentNotifications()
    {
      var logs = _repository.GetAllNotifications();

      // Use Linq to sort logs by latest first.
      return logs.OrderByDescending(log => log.NotificationPayload.SentAt).ToList();
    }
  }
}