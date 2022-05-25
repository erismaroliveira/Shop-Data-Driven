using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers;

[ApiController]
[Route("v1/api/[Controller]")]
public class ProductController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<Product>>> Get([FromServices] DataContext context)
    {
        var products = await context.Products
            .Include(x => x.Category)
            .AsNoTracking()
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<Product>> GetById(
        int id,
        [FromServices] DataContext context
    )
    {
        var product = await context.Products
            .Include(x => x.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        return Ok(product);
    }

    [HttpGet("categories/{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<Product>>> GetByCategory(
        int id,
        [FromServices] DataContext context
    )
    {
        var product = await context.Products
            .Include(x => x.Category)
            .AsNoTracking()
            .Where(x => x.CategoryId == id)
            .ToListAsync();

        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "employee")]
    public async Task<ActionResult<Product>> Post(
        [FromBody] Product model,
        [FromServices] DataContext context
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            context.Products.Add(model);
            await context.SaveChangesAsync();
            return Ok(model);
        }
        catch
        {
            return BadRequest(new { message = "Não foi possível criar o produto" });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "employee")]
    public async Task<ActionResult<Product>> Put(
        int id,
        [FromBody] Product model,
        [FromServices] DataContext context
    )
    {
        if (id != model.Id)
            return NotFound(new { message = "Produto não encontrado" });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            context.Entry<Product>(model).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return model;
        }
        catch (DbUpdateConcurrencyException)
        {
            return BadRequest(new { message = "Este registro já foi atualizado" });
        }
        catch (Exception)
        {
            return BadRequest(new { message = "Não foi possível atualizar o produto" });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "employee")]
    public async Task<ActionResult<Product>> Delete(
        int id,
        [FromServices] DataContext context
    )
    {
        var product = await context.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (product == null)
            return NotFound(new { message = "Produto não encontrado" });

        try
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync();
            return Ok(new { message = "Produto removido com sucesso" });
        }
        catch (Exception)
        {
            return BadRequest(new { message = "Não foi possível remover o produto" });
        }
    }
}