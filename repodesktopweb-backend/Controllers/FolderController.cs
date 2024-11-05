using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using repodesktopweb_backend.Models;

namespace repodesktopweb_backend.Controllers
{
    [Route("api/Folder")]
    [ApiController]
    [Authorize]
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
                var res = await _context.Folders.FirstOrDefaultAsync(x => x.NameFolder == modelFolder.NameFolder && x.Iduser == modelFolder.Iduser);

                if (res == null)
                {
                    _context.Folders.Add(modelFolder);
                    await _context.SaveChangesAsync();
                    var query = await _context.Folders.FirstOrDefaultAsync(x => x.NameFolder == modelFolder.NameFolder && x.Iduser == modelFolder.Iduser);
                    return Ok(query);
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

        [HttpGet]
        [Route("getFolder/{idUser}/{tipoFolder}/{idFolder?}")]
        public IActionResult GetFolder(  [FromRoute] int    idUser,
                                         [FromRoute] string tipoFolder,
                                         [FromRoute] int?   idFolder
                                       )
        {
            if (string.IsNullOrEmpty( tipoFolder ) || idUser == 0 )
            {
                return BadRequest("Debes completar los parametros solicitados para la consulta");
            }

            if (tipoFolder == "folder" && idFolder == 0)
            {
            
                var resFolder = _context.Folders
                                        .Where(x => x.Idfolder == null && x.Iduser == idUser)
                                        .ToList();
                
                if (!resFolder.Any())
                {
                    return Ok("No tienes carpetas creadas");
                }
                
                return Ok(resFolder);
            
            }
            
            else if (tipoFolder == "subfolder" && idFolder > 0)
            {
                
                var resSubFolder = _context.Folders.Where(x => x.Idfolder == idFolder && x.Iduser == idUser).ToList();
            
                if (!resSubFolder.Any())
                {
                    return Ok("No tienes sub carpetas creadas");
                }
                
                return Ok(resSubFolder);
            
            }

            else
            {
                return BadRequest("Debes completar los parametros solicitados para la consulta");
            }
        }

        [HttpDelete]
        [Route("deleteFolder/{id}")]
        public IActionResult DeleteFolder([FromRoute] int id)
        {
            var delete = _context.Folders.Where( x => x.Id == id).ExecuteDelete();
            return Ok("Carpeta eliminada correctamente");
        }

        [HttpPut]
        [Route("updateFolder/{id}")]
        public async Task<IActionResult> updateFolder( [FromRoute] int id, [FromBody] Folder folder )
        {
            
            if ( id != folder.Id )
            {
                return BadRequest("No existe la carpeta");
            }

            _context.Entry(folder).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(folder);


        }



    }
}
