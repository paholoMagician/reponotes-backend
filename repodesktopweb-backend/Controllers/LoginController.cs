using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using repodesktopweb_backend.Models;

[Route("api/Login")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly repofilesContext _context;
    private readonly IConfiguration _configuration;

    public LoginController(repofilesContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] Usuario userInfo)
    {
        var result = await _context.Usuarios
            .FirstOrDefaultAsync(x => x.Email == userInfo.Email && x.Password == userInfo.Password);

        if (result == null)
        {
            return BadRequest("Datos incorrectos");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity( new[] {
                new Claim(ClaimTypes.Name, result.Email),
                new Claim(ClaimTypes.Name, result.Nombre),
                new Claim(ClaimTypes.NameIdentifier, result.Id.ToString()),
                new Claim(ClaimTypes.Role, result.Rol.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new { Token = tokenString });
    }

}
