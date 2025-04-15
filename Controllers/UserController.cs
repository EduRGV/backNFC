using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NFC.Data;
using NFC.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/user
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users
            .Select(u => new User
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    // POST: api/users/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        // Validación básica
        if (string.IsNullOrEmpty(user.Username))
            return BadRequest("Username es requerido");

        if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            return Conflict("Username ya existe");

        // Hash de contraseña
        user.PasswordHash = HashPassword(user.PasswordHash);
        user.CreatedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    // GET: api/users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound();

        // Limpiamos datos sensibles antes de retornar
        user.PasswordHash = null;
        return user;
    }

    // PUT: api/users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User userData)
    {
        if (id != userData.Id) return BadRequest();

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        // Actualiza solo campos permitidos
        user.Username = userData.Username;
        user.Email = userData.Email;

        try
        {
            await _context.SaveChangesAsync();
            return new JsonResult(new { status = true, message = "Usuario actualizado" });
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    // DELETE: api/users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.Id == id);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }


    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] LoginRequest request)
    {
        var user = await _context.Users 
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
            return new JsonResult(new { status = false, message = "Usuario no encontrado" });


        var inputHash = HashPassword(request.Password);
        if (user.PasswordHash != inputHash)
            return new JsonResult(new { status = false, message = "Contraseña incorrecta" });

        user.PasswordHash = null;

        return new JsonResult(new { status = true, message = "Permiso permitido", user = user });
    }

}


public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}