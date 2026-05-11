using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedModels;
using SharedModels.Exceptions;
using SharedModels.Interfaces;
using BusinessLogic.NotificationSenders;

namespace BusinessLogic
{
  public class NotificationService
  {
    private readonly INotificationRepository _repository;
    private readonly UserService _userService;

    public NotificationService(INotificationRepository repository, UserService userService)
    {
      _repository = repository;
      _userService = userService;
    }

    public async Task ProcessAndSendNotificationAsync(int userId, string notificationType, string messageText)
    {
      if (string.IsNullOrWhiteSpace(messageText) || messageText.Length < 5)
        throw new ValidationException("Message must be at least 5 characters.");

      User user = await _userService.GetUserByIdAsync(userId);
      INotificationSender sender;

      if (notificationType.ToUpper() == "EMAIL")
      {
        if (string.IsNullOrWhiteSpace(user.Email) || !user.Email.Contains("@"))
          throw new ValidationException("User does not have a valid email.");
        sender = new EmailNotificationSender();
      }
      else if (notificationType.ToUpper() == "SMS")
      {
        if (string.IsNullOrWhiteSpace(user.PhoneNumber))
          throw new ValidationException("User does not have a valid phone number.");
        sender = new SmsNotificationSender();
      }
      else throw new ValidationException("Invalid type. Choose 'Email' or 'SMS'.");

      await sender.SendNotificationAsync(user, messageText);

      var log = new NotificationLog
      {
        UserId = user.Id,
        NotificationType = notificationType.ToUpper(),
        Message = messageText,
        SentAt = DateTime.Now
      };
      await _repository.SaveAsync(log);
    }

    public async Task<List<NotificationUserJoin>> GetSortedSentNotificationsAsync()
    {
      return await _repository.GetJoinedNotificationHistoryAsync();
    }
  }
}