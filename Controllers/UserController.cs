using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.Services;

namespace Shop.Controllers;

[ApiController]
[Route("v1/api/[Controller]")]
public class UserController : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "manager")]
    public async Task<ActionResult<List<User>>> Get(
        [FromBody] User model,
        [FromServices] DataContext context
    )
    {
        var users = await context.Users.AsNoTracking().ToListAsync();
        return users;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<User>> Post(
        [FromBody] User model,
        [FromServices] DataContext context
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Força o usuário a ser sempre "Funcionário"
            model.Role = "employee";

            context.Users.Add(model);
            await context.SaveChangesAsync();

            // Esconde a senha
            model.Password = "";
            return Ok(model);
        }
        catch (Exception)
        {
            return BadRequest(new { message = "Não foi possível cadastrar o usuário" });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "manager")]
    public async Task<ActionResult<User>> Put(
        int id,
        [FromBody] User model,
        [FromServices] DataContext context
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != model.Id)
            return NotFound(new { message = "Usuário não encontrado" });

        try
        {
            context.Entry(model).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return model;
        }
        catch (Exception)
        {
            return BadRequest(new { message = "Não foi possível alterar o usuário" });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<dynamic>> Authenticate(
        [FromBody] User model,
        [FromServices] DataContext context
    )
    {
        var user = await context.Users
            .AsNoTracking()
            .Where(x => x.UserName == model.UserName && x.Password == model.Password)
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new { message = "Usuário ou senha inválidos" });

        var token = TokenService.GenerateToken(user);
        // Esconde a senha
        user.Password = "";
        return new
        {
            user = user,
            token = token
        };
    }
}