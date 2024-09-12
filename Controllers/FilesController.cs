using DMS_TRAINING.Models;
using DMS_TRAINING.Services;
using Microsoft.AspNetCore.Mvc;
using DMS_TRAINING.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Reflection.Metadata;

namespace DMS_TRAINING.Controllers
{
    public class FilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOcrService _ocrService;

        public FilesController(ApplicationDbContext context, IOcrService ocrService)
        {
            _context = context;
            _ocrService = ocrService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var documents = await _context.FileMetadatas.ToListAsync();
            return View(documents);
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty", nameof(file));
            }

            var uploadDate = DateTime.Now;
            var monthFolder = $"{ uploadDate.ToString("MMMM")} { uploadDate.Year.ToString()}";
            var dayFolder = $"{monthFolder} {uploadDate.Day} {uploadDate.Year.ToString()}";

            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "Documents Uploaded",
                uploadDate.Year.ToString(),
                monthFolder,
                dayFolder
             );

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, file.Name);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var results = file.FileName.EndsWith(".pdf");
            var fileMetadata = new FileMetadata
            {
                        FileName = file.FileName,
                        FilePath = filePath,
                        UploadDate = DateTime.Now,
                        RecognitionText = null
            };
            
            using (var memoryStream = new MemoryStream())
             {
                await file.CopyToAsync(memoryStream);
                var fileData = new FileData
                {
                    Data = memoryStream.ToArray(),
                     FileMetadata = fileMetadata
                };
                
                _context.FileDatas.Add(fileData);
                _context.FileMetadatas.Add(fileMetadata);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
             }
         }

        [HttpGet]
        public async Task<IActionResult> GetFile(int id)
        {
            var fileData = await _context.FileDatas
        .Include(fd => fd.FileMetadata)
        .FirstOrDefaultAsync(fd => fd.FileMetadataId == id);

            if (fileData == null)
            {
                return NotFound();
            }

            var contentType = "application/pdf";
            var fileName = fileData.FileMetadata.FileName;

            // Setting the Content-Disposition header to inline
            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");

            return File(fileData.Data, contentType);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var document = await _context.FileMetadatas
                .Include(d => d.FileDatas)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound();
            }
            return View(document);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            var documents = await _context.FileMetadatas
                .Where(d => d.FileName.Contains(query))
                .ToListAsync();

            foreach (var document in documents.Where(d => string.IsNullOrEmpty(d.RecognitionText)))
            {
                await PerformOcrAndUpdateMetadata(document);
            }

            documents = await _context.FileMetadatas
                .Where(d => d.RecognitionText.Contains(query) || d.FileName.Contains(query))
                .ToListAsync();

            return View(documents);
        }

        private async Task PerformOcrAndUpdateMetadata(FileMetadata document)
        {
            var filePath = document.FilePath;

            var results = document.FileName.EndsWith(".pdf")
                ? await _ocrService.ExtractTextFromPdfAsync(filePath)
                : await _ocrService.ExtractTextFromImageAsync(new FileStream(filePath, FileMode.Open, FileAccess.Read));

            var text = string.Join("\n", results.Select(r => r.RecognitionText));

            document.RecognitionText = text;

            _context.FileMetadatas.Update(document);
            await _context.SaveChangesAsync();
        }


        //[HttpGet]
        //public async Task<IActionResult> Search(string query)
        //{
        //    if (string.IsNullOrEmpty(query))
        //    {
        //        return View(new List<FileMetadata>());
        //    }

        //    var documents = await _context.FileMetadatas
        //        //.Where(d => d.FileName.Contains(query) || (d.RecognitionText != null && d.RecognitionText.Contains(query)))
        //        .Where(d => d.RecognitionText != null && d.RecognitionText.Contains(query))
        //        .ToListAsync();

        //    if (documents == null)
        //    {
        //        _ocrService.PerformOcrAndUpdateMetadata();
        //    }


        //    return View(documents);
        //}

        //[HttpGet]
        //public async Task<IActionResult> Search(string query)
        //{
        //    // Fetch documents that match the query
        //    var documents = await _context.FileMetadatas
        //        .Where(d => (d.RecognitionText != null && d.RecognitionText.Contains(query)) || d.FileName.Contains(query))
        //        .ToListAsync();

        //    // Perform OCR on documents with null or empty RecognitionText
        //    foreach (var document in documents.Where(d => string.IsNullOrEmpty(d.RecognitionText)))
        //    {
        //        // Get the file path
        //        var filePath = document.FilePath;

        //        // Perform OCR based on the file type
        //        var results = document.FileName.EndsWith(".pdf")
        //            ? await _ocrService.ExtractTextFromPdfAsync(filePath)
        //            : await _ocrService.ExtractTextFromImageAsync(new FileStream(filePath, FileMode.Open, FileAccess.Read));

        //        // Combine recognition results into a single string
        //        var text = string.Join("\n", results.Select(r => r.RecognitionText));

        //        // Update the document's RecognitionText
        //        document.RecognitionText = text;
        //        _context.FileMetadatas.Update(document);
        //    }

        //    // Save changes after updating RecognitionText
        //    await _context.SaveChangesAsync();

        //    return View(documents);
        //}


        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _context.FileMetadatas
                .Include(d => d.FileDatas)
                .FirstOrDefaultAsync(d => d.Id == id);

            //            try
            //            {
            //                if (File.Exists(document.FilePath))
            //                {
            //                    File.Delete(document.FilePath);
            //                }

            //                // Remove the metadata from the database
            //                _context.FileMetadatas.Remove(document);
            //                await _context.SaveChangesAsync();
            //            }
            //            catch (IOException ex)
            //            {
            //                // Handle exceptions, log details, etc.
            //                // For example, log the error or return a custom error message
            //                Console.WriteLine($"An error occurred: {ex.Message}");
            //                return StatusCode(500, "Internal server error, please try again later.");
            //            }

            return RedirectToAction("Index");
        }
    }
}
