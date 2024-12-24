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
                    // Convertir el tamaño de bytes a gigabytes y asignarlo al campo `Size`
                    //modelFileServer.Size = modelFileServer.Size / 1073741824m; // 1 GB = 1073741824 bytes

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

        [HttpGet]
        [Route("GetFileServerDB/{idUser}/{idFolder?}")]
        public IActionResult GetFileServerDB([FromRoute] int idUser,
                                             [FromRoute] int idFolder)
        {
            var query = from fserv in _context.FileServers
                        join fl in _context.Folders on fserv.IdFolder equals fl.Id into folderGroup
                        from fl in folderGroup.DefaultIfEmpty() // Left join
                        where fserv.IdFolder == idFolder && fl.Iduser == idUser
                        select new
                        {
                            fserv.Id,
                            fserv.Position,
                            fserv.NameFile,
                            fserv.Tagdescription,
                            fserv.Estado,
                            fserv.Permisos,
                            fserv.Fecrea,
                            fserv.Password,
                            fserv.Type,
                            fserv.IdFolder,
                            fserv.Size,
                            fl.NameFolder,
                            fl.Descripcion
                        };


            if (query.Any())
            {
                var result = query.ToList();
                return Ok(result);
            }
            else
            {
                return BadRequest("No hay archivos guardados");
            }


        }


    }
}
