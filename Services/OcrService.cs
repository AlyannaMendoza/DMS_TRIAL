using Aspose.OCR;
using DMS_TRAINING.Data;
using DMS_TRAINING.Models;
using Microsoft.AspNetCore.Http;

namespace DMS_TRAINING.Services
{
    public interface IOcrService
    {
        //Task SaveFileAsync(IFormFile file);
        Task<List<RecognitionResult>> ExtractTextFromPdfAsync(string filePath);
        Task<List<RecognitionResult>> ExtractTextFromImageAsync(Stream imageStream);
        Task SaveSearchablePdfAsync(string inputPdfPath, string outputPdfPath);
        Task PerformOcrAndUpdateMetadata(int id);
    }

    public class OcrService : IOcrService
    {
        private readonly ApplicationDbContext _context;

        public OcrService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RecognitionResult>> ExtractTextFromPdfAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                // Create OcrInput for PDF
                var pdfInput = new OcrInput(InputType.PDF);
                pdfInput.Add(filePath);

                // Initialize the OCR engine
                var ocrEngine = new AsposeOcr();

                // Recognize the text from the document
                var results = ocrEngine.Recognize(pdfInput);
                return results;
            });
        }

        public async Task<List<RecognitionResult>> ExtractTextFromImageAsync(Stream imageStream)
        {
            return await Task.Run(() =>
            {
                // Save the image stream to a temporary file
                var tempFilePath = Path.GetTempFileName();
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                {
                    imageStream.CopyToAsync(fileStream);
                }

                // Create OcrInput for the image file
                var imageInput = new OcrInput(InputType.SingleImage);
                imageInput.Add(tempFilePath);

                // Initialize the OCR engine
                var ocrEngine = new AsposeOcr();
                var recognitionSettings = new RecognitionSettings();

                // Recognize text from image input
                var results = ocrEngine.Recognize(imageInput, recognitionSettings);

                // Clean up temporary file
                File.Delete(tempFilePath);

                return results;
            });
        }

        public async Task SaveSearchablePdfAsync(string inputPdfPath, string outputPdfPath)
        {
            await Task.Run(() =>
            {
                // Create OcrInput for PDF
                var pdfInput = new OcrInput(InputType.PDF);
                pdfInput.Add(inputPdfPath);

                // Initialize the OCR engine
                var ocrEngine = new AsposeOcr();

                // Recognize the text from the document
                var results = ocrEngine.Recognize(pdfInput);

                // Save searchable PDF
                AsposeOcr.SaveMultipageDocument(outputPdfPath, SaveFormat.Pdf, results);
            });
        }

        public async Task PerformOcrAndUpdateMetadata(int id)
        {
            var fileMetadata = await _context.FileMetadatas.FindAsync(id);
            if (fileMetadata != null)
            {
                // Perform OCR on the file
                var filePath = fileMetadata.FilePath;
                var text = await ExtractTextFromPdfAsync(filePath);  // Example method

                // Update metadata with OCR text
                fileMetadata.RecognitionText = text.ToString();
                await _context.SaveChangesAsync();
            }
        }


        //public async Task SaveFileAsync(IFormFile file)
        //{
        //            if (file == null || file.Length == 0)
        //            {
        //                throw new ArgumentException("File is empty", nameof(file));
        //    }

        //    var uploadDate = DateTime.Now;
        //    var yearFolder = uploadDate.Year.ToString();
        //    var monthFolder = uploadDate.ToString("MMMM");
        //    var dayFolder = $"{monthFolder} {uploadDate.Day} {yearFolder}";

        //    var folderPath = Path.Combine(
        //        Directory.GetCurrentDirectory(),
        //        "wwwroot",
        //        "Documents_Uploaded",
        //        yearFolder,
        //        monthFolder,
        //        dayFolder
        //     );

        //            if (!Directory.Exists(folderPath))
        //            {
        //                Directory.CreateDirectory(folderPath);
        //            }

        //var filePath = Path.Combine(folderPath, file.Name);

        //using (var stream = new FileStream(filePath, FileMode.Create))
        //{
        //    await file.CopyToAsync(stream);
        //}

        //var results = file.FileName.EndsWith(".pdf")
        //        ? await ExtractTextFromPdfAsync(filePath)
        //        : await ExtractTextFromImageAsync(file.OpenReadStream());

        //var text = string.Join("\n", results.Select(r => r.RecognitionText));

        //if (results == null || !results.Any())
        //{
        //    Handle the case where OCR extraction failed
        //    Console.WriteLine("OCR extraction returned null or empty results.");
        //    }

        //    if (string.IsNullOrEmpty(text))
        //    {
        //        Handle the case where the extracted text is empty
        //       Console.WriteLine("Extracted text is null or empty.");
        //        }

        //        var fileMetadata = new FileMetadata
        //        {
        //            FileName = file.FileName,
        //            FilePath = filePath,
        //            UploadDate = DateTime.Now,
        //            RecognitionText = text,
        //        };

        //        using (var memoryStream = new MemoryStream())
        //        {
        //            await file.CopyToAsync(memoryStream);
        //            var fileData = new FileData
        //            {
        //                Data = memoryStream.ToArray(),
        //                FileMetadata = fileMetadata
        //            };

        //            _context.FileDatas.Add(fileData);
        //            _context.FileMetadatas.Add(fileMetadata);
        //            await _context.SaveChangesAsync();
        //    }
        //}
    }
}