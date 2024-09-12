namespace DMS_TRAINING.Models
{
    public class FileMetadata
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string? RecognitionText { get; set; }
        public DateTime UploadDate { get; set; }
        public ICollection<FileData> FileDatas { get; set; }
    }
}
