using System.ComponentModel.DataAnnotations;

namespace Mahfazty.API.DTOs;

public class TransactionDto
{
    [Required, Range(0.01, 999999999)]
    public decimal Amount { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public DateTime? Date { get; set; }

    [MaxLength(200)]
    public string? Note { get; set; }
}