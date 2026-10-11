namespace Mahfazty.API.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public int UserId { get; set; }

    public User? User { get; set; }
    public List<Transaction> Transactions { get; set; } = new();
}