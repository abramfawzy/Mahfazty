namespace Mahfazty.API.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public List<Category> Categories { get; set; } = new();
    public List<Transaction> Transactions { get; set; } = new();
}