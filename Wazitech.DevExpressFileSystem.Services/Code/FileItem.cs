namespace Wazitech.DevExpressFileSystem.Services
{
    public class FileItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public bool IsDirectory { get; set; } = false;
        public byte[] Content { get; set; } = Array.Empty<byte>();

        public Guid ModifiedById { get; set; }
        public Guid ParentId { get; set; }

        public virtual User ModifiedBy { get; set; } = null!;
        public virtual FileItem Parent { get; set; } = null!;

        public virtual ICollection<FileItem> Files { get; set; } = new List<FileItem>();
    }
}
