using DMS_TRAINING.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DMS_TRAINING.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FileMetadata> FileMetadatas { get; set; }
        public DbSet<FileData>  FileDatas { get; set; }
    }
}
