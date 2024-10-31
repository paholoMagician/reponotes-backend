
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/storage")]
[ApiController]
[Authorize]
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

    [HttpPost("uploadFileDriveServer/{iduser}/{idFolder}")]
    public async Task<IActionResult> UploadFile([FromRoute] int iduser, [FromRoute] int idFolder, [FromForm] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("El archivo está vacío.");
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Tipo de archivo no permitido.");
            }

            var storagePath = Path.Combine(_hostingEnvironment.WebRootPath, storageFolder, iduser.ToString(), idFolder.ToString());

            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            var fileName = file.FileName.Replace(" ", "_");
            var filePath = Path.Combine(storagePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/{storageFolder}/{iduser}/{idFolder}/{fileName}";
            return Ok(new { message = "Archivo cargado con éxito", fileUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
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

