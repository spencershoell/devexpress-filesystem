﻿namespace Wazitech.DevExpressFileSystem.Services
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;

        public virtual ICollection<FileItem> Files { get; set; } = new List<FileItem>();
    }
}
