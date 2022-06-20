using Microsoft.EntityFrameworkCore;

namespace Wazitech.DevExpressFileSystem.Services
{
    public class FileManagementDbContext : DbContext
    {
        public virtual DbSet<FileItem> FileItems { get; set; } = null!;

        public FileManagementDbContext(DbContextOptions<FileManagementDbContext> options)
           : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
             .SelectMany(t => t.GetForeignKeys())
             .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
