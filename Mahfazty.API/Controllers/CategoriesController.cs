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
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _db.Categories
            .Where(c => c.UserId == CurrentUserId)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _db.Categories
            .Where(c => c.Id == id && c.UserId == CurrentUserId)
            .Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type
            })
            .FirstOrDefaultAsync();

        if (category is null)
            return NotFound(new { message = "Category not found" });

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CategoryDto dto)
    {
        var exists = await _db.Categories.AnyAsync(c =>
            c.UserId == CurrentUserId && c.Name == dto.Name && c.Type == dto.Type);

        if (exists)
            return BadRequest(new { message = "Category already exists" });

        var category = new Category
        {
            Name = dto.Name,
            Type = dto.Type,
            UserId = CurrentUserId
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        var response = new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type
        };

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CategoryDto dto)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == CurrentUserId);

        if (category is null)
            return NotFound(new { message = "Category not found" });

        category.Name = dto.Name;
        category.Type = dto.Type;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == CurrentUserId);

        if (category is null)
            return NotFound(new { message = "Category not found" });

        var hasTransactions = await _db.Transactions.AnyAsync(t => t.CategoryId == id);
        if (hasTransactions)
            return BadRequest(new { message = "Cannot delete a category that has transactions" });

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}