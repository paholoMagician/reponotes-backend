using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using orangebackend6.Controllers.files_controllers;
using repodesktopweb_backend.Models;
[Route("api/storage")]
[ApiController]
//[Authorize]
public class StorageController : ControllerBase
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly string[] allowedExtensions = new[] { ".bat" };
    private const string storageFolder = "storage";
    private readonly repofilesContext _context;
    public StorageController(IWebHostEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor, repofilesContext context)
    {
        _hostingEnvironment = hostingEnvironment;
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    /* RETORNA EL TAMANIO EN GB */
    [HttpGet("GetTotalFileSize/{userId}")]
    public IActionResult GetTotalFileSize(string userId)
    {
        // Ruta base de almacenamiento
        string userFolderPath = Path.Combine("wwwroot", "storage", userId);
        if (!Directory.Exists(userFolderPath))
        {
            return NotFound(new { message = "Carpeta de usuario no encontrada." });
        }

        // Calcular el tamaño total de los archivos en bytes
        long totalSizeInBytes = GetDirectorySize(userFolderPath);

        // Convertir el tamaño total a gigabytes (GB)
        double totalSizeInGB = totalSizeInBytes / (1024.0 * 1024.0 * 1024.0);

        // Devolver el tamaño total en gigabytes
        return Ok(new { totalSize = totalSizeInGB });

    }

    private long GetDirectorySize(string path)
    {
        long size = 0;

        // Obtener todos los archivos en el directorio y subdirectorios
        string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            FileInfo fileInfo = new FileInfo(file);
            size += fileInfo.Length;
        }

        return size;
    }


    [HttpGet("getFileChunk/{email}/{folderName}/{fileName}")]
    public IActionResult GetFileChunk(string email, string folderName, string fileName, [FromHeader(Name = "Range")] string rangeHeader)
    {
        string fileModelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "storage");
        string folderUserPath = Path.Combine(fileModelPath, email);
        string folderFilePath = Path.Combine(folderUserPath, folderName);
        string filePath = Path.Combine(folderFilePath, fileName);
        try
        {
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new
                {
                    message = "Archivo no encontrado."
                });
            }
            FileInfo fileInfo = new FileInfo(filePath);
            long fileLength = fileInfo.Length;

            if (string.IsNullOrEmpty(rangeHeader))
            {
                return BadRequest(new
                {
                    message = "Encabezado Range requerido."
                });
            }
            string[] range = rangeHeader.Replace("bytes=", "").Split('-');
            if (!long.TryParse(range[0], out var start))
            {
                return BadRequest(new
                {
                    message = "Rango de inicio no válido."
                });
            }
            long parsedEnd;
            long end = ((range.Length > 1 && long.TryParse(range[1], out parsedEnd)) ? parsedEnd : (fileLength - 1));
            if (start >= fileLength || end >= fileLength || start > end)
            {
                return BadRequest(new
                {
                    message = "Rango no válido."
                });
            }
            long chunkSize = end - start + 1;
            byte[] buffer = new byte[chunkSize];
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fileStream.Seek(start, SeekOrigin.Begin);
                fileStream.Read(buffer, 0, buffer.Length);
            }
            base.Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{fileLength}");
            base.Response.Headers.Add("Accept-Ranges", "bytes");
            return File(buffer, GetContentType(filePath), fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = "Error al obtener el archivo.",
                error = ex.Message
            });
        }
    }




    private string GetContentType(string path)
    {
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(path, out var contentType))
        {
            contentType = "application/octet-stream";
        }
        return contentType;
    }




    [HttpPost("uploadFileDriveServer/{email}/{folderName}")]
    public async Task<IActionResult> uploadFileDriveServer([FromForm] IMGmodelClass request, [FromRoute] string email, [FromRoute] string folderName)
    {
        string fileModelpath = Path.Combine(Directory.GetCurrentDirectory() + "\\wwwroot", "storage");
        string folderUserPath = Path.Combine(fileModelpath, email);
        string folderFilePath = Path.Combine(folderUserPath, folderName);
        Console.WriteLine("INICIANDO API FILE 1");
        Console.WriteLine(request);
        try
        {
            Console.WriteLine("INICIANDO API FILE 2");
            if (!Directory.Exists(fileModelpath))
            {
                Directory.CreateDirectory(fileModelpath);
            }
            if (!Directory.Exists(folderUserPath))
            {
                Directory.CreateDirectory(folderUserPath);
            }
            if (!Directory.Exists(folderFilePath))
            {
                Directory.CreateDirectory(folderFilePath);
            }
            if (request.Archivo is null)
            {
                Console.WriteLine("NO HAY ARCHIVO");
                return BadRequest("NO HAY ARCHIVO");
            }
            Console.WriteLine("INICIANDO API FILE 3");
            // Utilizar el nombre original del archivo sin el sufijo
            string newFileName = request.Archivo.FileName;
            string filePath = Path.Combine(folderFilePath, newFileName);
            using FileStream newFile = System.IO.File.Create(filePath);
            await request.Archivo.CopyToAsync(newFile);
            await newFile.FlushAsync(); return Ok();
        }
        catch (Exception err)
        {
            return BadRequest(err);
        }
    }
    [HttpPost("uploadPerfil")]
    public async Task<IActionResult> uploadPerfil()
    {
        try
        {
            var file = _httpContextAccessor.HttpContext.Request.Form.Files[0];
            if (file.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Tipo de archivo no permitido.");
                }
                var storagePath = Path.Combine(_hostingEnvironment.WebRootPath, "perfil");
                // Utiliza el nombre original del archivo
                var originalFileName = file.FileName;
                // Reemplaza espacios en blanco por guiones bajos
                var fileName = originalFileName.Replace(" ", "_");
                var filePath = Path.Combine(storagePath, fileName);
                using (
                    var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                // Puedes devolver la URL del archivo como respuesta.
                var fileUrl = $"/perfil/{fileName}";
                return Ok(new
                {
                    message = "Archivo cargado con éxito",
                    fileUrl
                }
                );
            }
            else
            {
                return BadRequest("El archivo está vacío.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    [HttpDelete("DeleteFolder/{userId}/{folderId}")]
    public IActionResult DeleteFolder(string userId, string folderId)
    {
        // Ruta de la carpeta a eliminar
        string folderPath = Path.Combine("wwwroot", "storage", userId, folderId);
        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, true
                );
            // 'true' para eliminar también subdirectorios y archivos
            return Ok(new
            {
                message = "Carpeta eliminada exitosamente."
            });
        }
        else
        {
            return NotFound(new { message = "Carpeta no encontrada." });
        }
    }


    [HttpDelete("DeleteFile/{userId}/{folderId}/{fileName}")]
    public IActionResult DeleteFile(string userId, string folderId, string fileName)
    {
        // Ruta del archivo a eliminar
        string filePath = Path.Combine("wwwroot", "storage", userId, folderId, fileName);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
            // Eliminar el archivo
            return Ok(new { message = "Archivo eliminado exitosamente." });
        }
        else
        {
            return NotFound(new { message = "Archivo no encontrado." });
        }
    }

    [HttpDelete("DeleteFileDB/{idfile}")]
    public IActionResult DeleteFileDB(int idfile)
    {
        var file = _context.FileServers.FirstOrDefault(f => f.Id == idfile);
        if (file == null)
        {
            return NotFound(new { message = "Archivo no encontrado." });
        }
        _context.FileServers.Remove(file);
        // Usamos Remove en lugar de RemoveRange
        _context.SaveChanges();
        return Ok(new { message = "Archivo eliminado correctamente." });
    }

    [HttpDelete("DeleteFolderDB/{folderId}")]
    public IActionResult DeleteFolderDB(int folderId)
    {
        var folder = _context.Folders.FirstOrDefault(f => f.Id == folderId);
        var filesInFolder = _context.FileServers.Where(x => x.IdFolder == folderId).ToList();
        if (folder == null)
        {
            return NotFound(new { message = "Carpeta no encontrada." });
        }
        // Eliminar los archivos relacionados
        _context.FileServers.RemoveRange(filesInFolder);
        // Eliminar la carpeta
        _context.Folders.Remove(folder);
        _context.SaveChanges();
        return Ok(new { message = "Carpeta y archivos eliminados exitosamente." });
    }
}





