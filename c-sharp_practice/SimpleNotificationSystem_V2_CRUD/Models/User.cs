namespace SimpleNotificationSystem_V2_CRUD.Models
{
  public class User
  {
    // New Id field added to each user for CURD (class joke) operations
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
  }
}