using System.Security.Claims;
using Mahfazty.API.Data;
using Mahfazty.API.DTOs;
using Mahfazty.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mahfazty.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TransactionsController(AppDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll(DateTime? from, DateTime? to, int? categoryId)
    {
        var query = _db.Transactions
            .Where(t => t.UserId == CurrentUserId);

        if (from.HasValue)
            query = query.Where(t => t.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(t => t.Date < to.Value.Date.AddDays(1));

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        var result = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Select(t => new TransactionResponseDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Type = t.Type,
                Date = t.Date,
                Note = t.Note,
                CategoryId = t.CategoryId,
                CategoryName = t.Category!.Name
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var transaction = await _db.Transactions
            .Where(t => t.Id == id && t.UserId == CurrentUserId)
            .Select(t => new TransactionResponseDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Type = t.Type,
                Date = t.Date,
                Note = t.Note,
                CategoryId = t.CategoryId,
                CategoryName = t.Category!.Name
            })
            .FirstOrDefaultAsync();

        if (transaction is null)
            return NotFound(new { message = "Transaction not found" });

        return Ok(transaction);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TransactionDto dto)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.UserId == CurrentUserId);

        if (category is null)
            return BadRequest(new { message = "Category not found" });

        var transaction = new Transaction
        {
            Amount = dto.Amount,
            Type = category.Type,
            Date = dto.Date ?? DateTime.Now,
            Note = dto.Note,
            CategoryId = category.Id,
            UserId = CurrentUserId
        };

        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();

        var response = new TransactionResponseDto
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Type = transaction.Type,
            Date = transaction.Date,
            Note = transaction.Note,
            CategoryId = category.Id,
            CategoryName = category.Name
        };

        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TransactionDto dto)
    {
        var transaction = await _db.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

        if (transaction is null)
            return NotFound(new { message = "Transaction not found" });

        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.UserId == CurrentUserId);

        if (category is null)
            return BadRequest(new { message = "Category not found" });

        transaction.Amount = dto.Amount;
        transaction.Type = category.Type;
        transaction.CategoryId = category.Id;
        transaction.Date = dto.Date ?? transaction.Date;
        transaction.Note = dto.Note;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var transaction = await _db.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

        if (transaction is null)
            return NotFound(new { message = "Transaction not found" });

        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync();

        return NoContent();
    }

        [HttpGet("summary/today")]
    public async Task<IActionResult> Today()
    {
        var from = DateTime.Today;
        var to = from.AddDays(1);
        return Ok(await BuildSummary(from, to));
    }

    [HttpGet("summary/month")]
    public async Task<IActionResult> Month(int? year, int? month)
    {
        var now = DateTime.Today;
        var y = year ?? now.Year;
        var m = month ?? now.Month;

        if (m < 1 || m > 12)
            return BadRequest(new { message = "Month must be between 1 and 12" });

        var from = new DateTime(y, m, 1);
        var to = from.AddMonths(1);
        return Ok(await BuildSummary(from, to));
    }

    private async Task<SummaryDto> BuildSummary(DateTime from, DateTime to)
    {
        var totals = await _db.Transactions
            .Where(t => t.UserId == CurrentUserId && t.Date >= from && t.Date < to)
            .GroupBy(t => t.Type)
            .Select(g => new { Type = g.Key, Total = g.Sum(t => t.Amount) })
            .ToListAsync();

        var income = totals.FirstOrDefault(x => x.Type == TransactionType.Income)?.Total ?? 0;
        var expense = totals.FirstOrDefault(x => x.Type == TransactionType.Expense)?.Total ?? 0;

        return new SummaryDto
        {
            From = from,
            To = to.AddDays(-1),
            TotalIncome = income,
            TotalExpense = expense,
            Balance = income - expense
        };
    }
}