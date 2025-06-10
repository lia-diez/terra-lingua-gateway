namespace Api.DB.Models;

public class ApiKey
{
    public int KeyId { get; set; }
    public int UserId { get; set; }
    public string KeyString { get; set; } = null!;
    public DateTime ValidTill { get; set; }
    public User User { get; set; } = null!;
}