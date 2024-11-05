
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using orangebackend6.Controllers.files_controllers;

[Route("api/storage")]
[ApiController]
//[Authorize]
public class StorageController : ControllerBase
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly string[] allowedExtensions = new[] { ".bat" };
    private const string storageFolder = "storage";

    public StorageController(IWebHostEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor)
    {
        _hostingEnvironment = hostingEnvironment;
        _httpContextAccessor = httpContextAccessor;

    }



    [HttpGet("getFile/{email}/{folderName}/{fileName}")]
    public IActionResult GetFile(string email, string folderName, string fileName)
    {
        // Ruta base de almacenamiento
        string fileModelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "storage");
        // Ruta de carpeta del usuario
        string folderUserPath = Path.Combine(fileModelPath, email);
        // Ruta completa del archivo
        string folderFilePath = folderName == "VOID" ? folderUserPath : Path.Combine(folderUserPath, folderName);
        string filePath = Path.Combine(folderFilePath, fileName);

        try
        {
            // Verificar si el archivo existe
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = "Archivo no encontrado." });
            }

            // Obtener el tipo de contenido MIME para el archivo
            var contentType = GetContentType(filePath);
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Devolver el archivo como un FileResult
            return File(fileBytes, contentType, fileName);
        }
        catch (Exception err)
        {
            return BadRequest(new { message = "Error al obtener el archivo.", error = err.Message });
        }
    }

    // Método auxiliar para obtener el tipo de contenido MIME basado en la extensión del archivo
    private string GetContentType(string path)
    {
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(path, out var contentType))
        {
            contentType = "application/octet-stream";
        }
        return contentType;
    }


    //[HttpPost("uploadFileDriveServer/{email}/{folderName}")]
    //public async Task<IActionResult> uploadFileDriveServer([FromForm] IMGmodelClass request, [FromRoute] string email, [FromRoute] string folderName)
    //{
    //    // Ruta base de almacenamiento
    //    string fileModelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "storage");
    //    // Ruta de carpeta del usuario
    //    string folderUserPath = Path.Combine(fileModelPath, email);
    //    // Ruta de carpeta específica para el archivo
    //    string folderFilePath = folderName == "VOID" ? folderUserPath : Path.Combine(folderUserPath, folderName);

    //    try
    //    {
    //        // Crear directorios si no existen
    //        if (!Directory.Exists(fileModelPath))
    //        {
    //            Directory.CreateDirectory(fileModelPath);
    //        }

    //        if (!Directory.Exists(folderUserPath))
    //        {
    //            Directory.CreateDirectory(folderUserPath);
    //        }

    //        if (folderName != "VOID" && !Directory.Exists(folderFilePath))
    //        {
    //            Directory.CreateDirectory(folderFilePath);
    //        }

    //        if (request.Archivo is not null)
    //        {
    //            // Generar un sufijo único para el archivo
    //            string uniqueSuffix = Guid.NewGuid().ToString();
    //            string newFileName = $"{Path.GetFileNameWithoutExtension(request.Archivo.FileName)}_{uniqueSuffix}{Path.GetExtension(request.Archivo.FileName)}";

    //            // Definir la ruta del archivo según si `folderName` es "VOID" o no
    //            string filePath = folderName == "VOID" ? Path.Combine(folderUserPath, newFileName) : Path.Combine(folderFilePath, newFileName);

    //            // Guardar el archivo
    //            using FileStream newFile = System.IO.File.Create(filePath);
    //            await request.Archivo.CopyToAsync(newFile);
    //            await newFile.FlushAsync();
    //        }

    //        return Ok();
    //    }
    //    catch (Exception err)
    //    {
    //        return BadRequest(err);
    //    }
    //}


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

            if(request.Archivo is null)
            {
                Console.WriteLine("NO HAY ARCHIVO");
                return BadRequest("NO HAY ARCHIVO");
            }

            //if (request.Archivo is not null)
            //{
                Console.WriteLine("INICIANDO API FILE 3");
                // Generar un sufijo único para el archivo
                string uniqueSuffix = Guid.NewGuid().ToString();
                string newFileName = $"{Path.GetFileNameWithoutExtension(request.Archivo.FileName)}_{uniqueSuffix}{Path.GetExtension(request.Archivo.FileName)}";

                string filePath = Path.Combine(folderFilePath, newFileName);

                using FileStream newFile = System.IO.File.Create(filePath);
                await request.Archivo.CopyToAsync(newFile);
                await newFile.FlushAsync();
        //}

            return Ok();
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

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Puedes devolver la URL del archivo como respuesta.
                var fileUrl = $"/perfil/{fileName}";

                return Ok(new { message = "Archivo cargado con éxito", fileUrl });
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


}

