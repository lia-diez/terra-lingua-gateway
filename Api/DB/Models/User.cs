namespace Api.DB.Models;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string HashedPwd { get; set; } = null!;

    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
}