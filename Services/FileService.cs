//using DMS_TRAINING.Data;
//using DMS_TRAINING.Models;
//using Microsoft.EntityFrameworkCore;

//namespace DMS_TRAINING.Services
//{
//    public interface IFileService
//    {
//        Task UploadFileAsync(IFormFile file);
//        Task<List<FileMetadata>> GetFileMetadataAsync();
//        Task<FileMetadata> GetFileDataAsync(int id);
//    }

//    public class FileService : IFileService
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IWebHostEnvironment _environment;

//        public FileService(ApplicationDbContext context, IWebHostEnvironment environment)
//        {
//            _context = context;
//            _environment = environment;
//        }

//        public async Task UploadFileAsync(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//                throw new ArgumentException("File cannot be null or empty");

//            var uploadDate = DateTime.Now;
//            var folderPath = Path.Combine(_environment.WebRootPath, "uploads", uploadDate.Year.ToString(), uploadDate.Month.ToString(), uploadDate.Day.ToString());

//            if (!Directory.Exists(folderPath))
//            {
//                Directory.CreateDirectory(folderPath);
//            }

//            var filePath = Path.Combine(folderPath, file.Name);

//            using (var stream = new FileStream(filePath, FileMode.Create))
//            {
//                await file.CopyToAsync(stream);
//            }

//            var fileMetadata = new FileMetadata
//            {
//                FileName = file.FileName,
//                ContentType = file.ContentType,
//                UploadDate = uploadDate,
//                FilePath = filePath
//            };

//            _context.FileMetadatas.Add(fileMetadata);
//            await _context.SaveChangesAsync();
//        }

//        public async Task<List<FileMetadata>> GetFileMetadataAsync()
//        {
//            return await _context.FileMetadatas.ToListAsync();
//        }

//        //public async Task<FileData> GetFileDataAsync(int id)
//        //{
//        //    return await _context.FileDatas
//        //        .Include(fd => fd.FileMetadata)
//        //        .FirstOrDefaultAsync(fd => fd.Id == id);
//        //}

//        public async Task<FileMetadata> GetMetadataAsync(int id)
//        {
//            return await _context.FileMetadatas.FindAsync(id);
//        }

//        public Task<FileMetadata> GetFileDataAsync(int id)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
