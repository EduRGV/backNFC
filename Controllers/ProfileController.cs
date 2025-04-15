using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NFC.Data;
using NFC.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProfileController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Profile
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Profile>>> GetProfiles()
    {
        return await _context.Profiles.ToListAsync();
    }

    private bool ProfileExists(int id)
    {
        return _context.Users.Any(e => e.Id == id);
    }

    // GET: api/Profile/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProfile(int id)
    {
        var profile = await _context.Profiles.FindAsync(id);
        if (profile == null) return NotFound();

        if (!string.IsNullOrEmpty(profile.ImageUrl))
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            profile.ImageUrl = $"{baseUrl}{profile.ImageUrl}";
        }

        if (!string.IsNullOrEmpty(profile.Background))
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            profile.Background = $"{baseUrl}{profile.Background}";
        }

        return Ok(profile);
    }

    // POST: api/Profile
    [HttpPost]
    public async Task<ActionResult<Profile>> CreateProfile(
        [FromForm] Profile profile,
        IFormFile? imageFile,
        IFormFile? backgroundFile)
    {
        if (profile == null)
        {
            return BadRequest("El perfil no puede ser nulo.");
        }
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        Directory.CreateDirectory(uploadsFolder);


        if (imageFile != null && imageFile.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            profile.ImageUrl = $"/images/{fileName}";
            Console.WriteLine($"✅ Imagen principal guardada: {filePath}");
        }

        if (backgroundFile != null && backgroundFile.Length > 0)
        {
            var bgFileName = $"{Guid.NewGuid()}_{backgroundFile.FileName}";
            var bgFilePath = Path.Combine(uploadsFolder, bgFileName);

            using (var stream = new FileStream(bgFilePath, FileMode.Create))
            {
                await backgroundFile.CopyToAsync(stream);
            }

            profile.Background = $"/images/{bgFileName}";
            Console.WriteLine($"✅ Imagen de background guardada: {bgFilePath}");
        }

        _context.Profiles.Add(profile);
        await _context.SaveChangesAsync();

        profile.ProfileUrl = $"http://localhost:3000/profile/{profile.Id}";
        _context.Profiles.Update(profile);
        await _context.SaveChangesAsync();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        if (!string.IsNullOrEmpty(profile.ImageUrl))
            profile.ImageUrl = $"{baseUrl}{profile.ImageUrl}";
        if (!string.IsNullOrEmpty(profile.Background))
            profile.Background = $"{baseUrl}{profile.Background}";

        return CreatedAtAction(nameof(GetProfile), new { id = profile.Id }, profile);
    }



    // PUT: api/Profile/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProfile(int id, [FromBody] Profile userData)
    {
        if (id != userData.Id) return BadRequest();

        var user = await _context.Profiles.FindAsync(id);
        if (user == null) return NotFound();

        // Actualiza solo campos permitidos
        user.Name = userData.Name;
        user.Email = userData.Email;
        user.Description= userData.Description;
        user.PhoneNumber = userData.PhoneNumber;
        user.Position= userData.Position;

        try
        {
            await _context.SaveChangesAsync();
            return new JsonResult(new { status = true, message = "Perfil del Usuario actualizado" });
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProfileExists(id)) return NotFound();
            throw;
        }

        return NoContent();
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProfile(int id)
    {
        var user = await _context.Profiles.FindAsync(id);
        if (user == null) return NotFound();

        _context.Profiles.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }



}
