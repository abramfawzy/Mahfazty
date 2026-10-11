using System.ComponentModel.DataAnnotations;
using Mahfazty.API.Models;

namespace Mahfazty.API.DTOs;

public class CategoryDto
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public TransactionType Type { get; set; }
}