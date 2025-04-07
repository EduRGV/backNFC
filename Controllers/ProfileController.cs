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

    // GET: api/Profile/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProfile(int id)
    {
        var profile = await _context.Profiles.FindAsync(id);
        if (profile == null) return NotFound();

        // Verificar si tiene una imagen y generar la URL absoluta
        if (!string.IsNullOrEmpty(profile.ImageUrl))
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            profile.ImageUrl = $"{baseUrl}{profile.ImageUrl}";
        }

        return Ok(profile);
    }


    // POST: api/Profile
    [HttpPost]
    public async Task<ActionResult<Profile>> CreateProfile([FromForm] Profile profile, IFormFile? imageFile)
    {
        if (profile == null)
        {
            return BadRequest("El perfil no puede ser nulo.");
        }

        // Guardar la imagen si se envió una
        if (imageFile != null && imageFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            Directory.CreateDirectory(uploadsFolder); // Asegura que la carpeta exista

            var fileName = $"{Guid.NewGuid()}_{imageFile.FileName}"; // Nombre único
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Guardamos la ruta relativa de la imagen
            profile.ImageUrl = $"/images/{fileName}";
            Console.WriteLine($"✅ Imagen guardada en: {filePath}");
        }
        else
        {
            Console.WriteLine("⚠️ No se recibió ninguna imagen.");
        }

        // 🔹 Asignamos la URL del perfil antes de guardarlo en la BD
        _context.Profiles.Add(profile);
        await _context.SaveChangesAsync(); // Aquí se genera el ID

        profile.ProfileUrl = $"http://localhost:3000/profile/{profile.Id}";

        // 🔹 Guardamos la URL del perfil en la BD
        _context.Profiles.Update(profile);
        await _context.SaveChangesAsync();

        Console.WriteLine($"✅ Perfil guardado con ID: {profile.Id} y URL: {profile.ProfileUrl}");

        // Crear la URL absoluta de la imagen para la respuesta
        if (!string.IsNullOrEmpty(profile.ImageUrl))
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            profile.ImageUrl = $"{baseUrl}{profile.ImageUrl}";
            Console.WriteLine($"✅ URL completa de la imagen: {profile.ImageUrl}");
        }

        return CreatedAtAction(nameof(GetProfile), new { id = profile.Id }, profile);
    }



    // GET: api/Profile/Url/5
    [HttpGet("Url/{id}")]
    public async Task<ActionResult<string>> GetProfileUrl(int id)
    {
        // Buscar el perfil por ID
        var profile = await _context.Profiles.FindAsync(id);

        // Verificar si el perfil existe
        if (profile == null)
        {
            return NotFound(); // Retorna 404 si no encuentra el perfil
        }

        // Retornar solo el campo ProfileUrl
        return Ok(profile.ProfileUrl);
    }






}
