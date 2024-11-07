using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using repodesktopweb_backend.Models;

namespace repodesktopweb_backend.Controllers
{
    [Route("api/filesDB")]
    [ApiController]
    [Authorize]
    public class FilesDBController : ControllerBase
    {

        private readonly repofilesContext _context;

        public FilesDBController(repofilesContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("FileCreate")]
        public async Task<IActionResult> FileCreate([FromBody] FileServer modelFileServer)
        {
            if (ModelState.IsValid)
            {
                var res = await _context.FileServers.FirstOrDefaultAsync(x => x.NameFile == modelFileServer.NameFile && x.IdFolder == modelFileServer.IdFolder);

                if (res == null)
                {
                    _context.FileServers.Add(modelFileServer);
                    await _context.SaveChangesAsync();
                    var query = await _context.FileServers.FirstOrDefaultAsync(x => x.NameFile == modelFileServer.NameFile && x.IdFolder == modelFileServer.IdFolder);
                    return Ok(query);
                }
                else
                {
                    return BadRequest("El archivo ya existe.");
                }
            }
            else
            {
                return BadRequest("Modelo de archivo inválido");
            }
        }


    }
}
