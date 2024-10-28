using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using repodesktopweb_backend.Models;

namespace repodesktopweb_backend.Controllers
{
    [Route("api/Folder")]
    [ApiController]
    [Authorize] // Esto requiere que las solicitudes estén autenticadas
    public class FolderController : ControllerBase
    {
        private readonly repofilesContext _context;

        public FolderController(repofilesContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("FolderCreate")]
        public async Task<IActionResult> FolderCreate([FromBody] Folder modelFolder)
        {
            if (ModelState.IsValid)
            {
                var res = await _context.Folders.FirstOrDefaultAsync(x => x.NameFolder == modelFolder.NameFolder);
                if (res == null)
                {
                    _context.Folders.Add(modelFolder);
                    await _context.SaveChangesAsync();
                    return Ok(modelFolder);
                }
                else
                {
                    return BadRequest("La carpeta ya existe.");
                }
            }
            else
            {
                return BadRequest("Modelo de carpeta inválido");
            }
        }
    }
}
