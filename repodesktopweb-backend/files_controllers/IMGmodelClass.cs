namespace orangebackend6.Controllers.files_controllers
{
    public class IMGmodelClass
    {
        public IFormFile Archivo { get; set; }
        public string FileName { get; set; }
        public int CurrentChunk { get; set; }
        public int TotalChunks { get; set; }
    }
}
