using DevExtreme.AspNet.Mvc.FileManagement;
using Microsoft.EntityFrameworkCore;

namespace Wazitech.DevExpressFileSystem.Services
{
    public class DbFileProvider : IFileSystemItemLoader, IFileSystemItemEditor, IFileUploader, IFileContentProvider
    {
        static readonly Guid GuestPersonId = Guid.Parse("2a5eb726-ef1a-42ab-b91d-cdd268c69865");

        public DbFileProvider(FileManagementDbContext fileManagementDbContext)
        {
            FileManagementDbContext = fileManagementDbContext;
        }

        protected FileManagementDbContext FileManagementDbContext { get; }

        public IEnumerable<FileSystemItem> GetItems(FileSystemLoadItemOptions options)
        {
            var parentId = ParseKey(options.Directory.Key);
            var fileItems = GetDirectoryContents(parentId);
            var hasSubDirectoriesInfo = GetHasSubDirectoriesInfo(fileItems).GetAwaiter().GetResult();

            var clientItemList = new List<FileSystemItem>();
            foreach (var item in fileItems)
            {
                var clientItem = new FileSystemItem
                {
                    Key = item.Id.ToString(),
                    Name = item.Name,
                    IsDirectory = item.IsDirectory,
                    DateModified = item.Modified
                };

                if (item.IsDirectory)
                {
                    clientItem.HasSubDirectories = hasSubDirectoriesInfo.ContainsKey(item.Id) && hasSubDirectoriesInfo[item.Id];
                }

                clientItem.CustomFields["modifiedBy"] = item.ModifiedBy.FullName;
                clientItem.CustomFields["created"] = item.Created;
                clientItemList.Add(clientItem);
            }
            return clientItemList;
        }

        public void CreateDirectory(FileSystemCreateDirectoryOptions options)
        {
            var parentDirectory = options.ParentDirectory;
            if (!IsFileItemExists(parentDirectory))
                ThrowItemNotFoundException(parentDirectory);

            var directory = new FileItem
            {
                Name = options.DirectoryName,
                Modified = DateTime.Now,
                Created = DateTime.Now,
                IsDirectory = true,
                ModifiedById = GuestPersonId,
                ParentId = Guid.Empty
            };

            var parentId = ParseKey(parentDirectory.Key);

            if (parentId != Guid.Empty)
                directory.ParentId = parentId;

            FileManagementDbContext.FileItems.Add(directory);
            FileManagementDbContext.SaveChanges();
        }

        public void RenameItem(FileSystemRenameItemOptions options)
        {
            var item = options.Item;

            if (!IsFileItemExists(item))
                ThrowItemNotFoundException(item);

            var fileItem = GetFileItem(item);
            if (fileItem != null)
            {
                fileItem.Name = options.ItemNewName;
                fileItem.ModifiedById = GuestPersonId;
                fileItem.Modified = DateTime.Now;
                FileManagementDbContext.SaveChanges();
            }
        }

        public void MoveItem(FileSystemMoveItemOptions options)
        {
            var item = options.Item;
            var destinationDirectory = options.DestinationDirectory;

            if (!IsFileItemExists(item))
                ThrowItemNotFoundException(item);
            if (!IsFileItemExists(destinationDirectory))
                ThrowItemNotFoundException(destinationDirectory);
            if (!AllowCopyOrMove(item, destinationDirectory))
                ThrowNoAccessException();

            var fileItem = GetFileItem(item);
            if (fileItem != null)
            {
                fileItem.ParentId = ParseKey(destinationDirectory.Key);
                fileItem.Modified = DateTime.Now;
                fileItem.ModifiedById = GuestPersonId;
                FileManagementDbContext.SaveChanges();
            }
        }

        public void CopyItem(FileSystemCopyItemOptions options)
        {
            var item = options.Item;
            var destinationDirectory = options.DestinationDirectory;

            if (!IsFileItemExists(item))
                ThrowItemNotFoundException(item);
            if (!IsFileItemExists(destinationDirectory))
                ThrowItemNotFoundException(destinationDirectory);
            if (!AllowCopyOrMove(item, destinationDirectory))
                ThrowNoAccessException();

            var sourceFileItem = GetFileItem(item);
            var copyFileItem = CreateCopy(sourceFileItem);
            if (copyFileItem != null && sourceFileItem != null)
            {
                copyFileItem.ParentId = ParseKey(destinationDirectory.Key);
                copyFileItem.Name = GenerateCopiedFileItemName(copyFileItem.ParentId, copyFileItem.Name, copyFileItem.IsDirectory);
                FileManagementDbContext.FileItems.Add(copyFileItem);

                if (copyFileItem.IsDirectory)
                    CopyDirectoryContentRecursive(sourceFileItem, copyFileItem);
                FileManagementDbContext.SaveChanges();
            }
        }

        void CopyDirectoryContentRecursive(FileItem sourcePathInfo, FileItem destinationPathInfo)
        {
            foreach (var fileItem in GetDirectoryContents(sourcePathInfo.Id))
            {
                var copyItem = CreateCopy(fileItem);
                if (copyItem != null)
                {
                    copyItem.Parent = destinationPathInfo;
                    FileManagementDbContext.FileItems.Add(copyItem);
                    if (fileItem.IsDirectory)
                        CopyDirectoryContentRecursive(fileItem, copyItem);
                }
            }
        }

        // GTG
        public void DeleteItem(FileSystemDeleteItemOptions options)
        {
            var item = options.Item;

            if (!IsFileItemExists(item))
                ThrowItemNotFoundException(item);

            var fileItem = GetFileItem(item);

            if (fileItem != null)
            {
                if (fileItem.IsDirectory)
                    RemoveDirectoryContentRecursive(fileItem.Id);

                FileManagementDbContext.FileItems.Remove(fileItem);

                FileManagementDbContext.SaveChangesAsync().GetAwaiter().GetResult();
            }
        }

        public void RemoveDirectoryContentRecursive(Guid parentDirectoryKey)
        {
            var itemsToRemove = FileManagementDbContext.FileItems
                .Where(e => e.ParentId == parentDirectoryKey);

            foreach (var item in itemsToRemove)
                if (item.IsDirectory)
                    RemoveDirectoryContentRecursive(item.Id);

            foreach (var item in itemsToRemove)
                FileManagementDbContext.FileItems.Remove(item);
        }

        IQueryable<FileItem> GetDirectoryContents(Guid parentKey)
        {
            var query = FileManagementDbContext.FileItems
                .Include(e => e.ModifiedBy)
                .Include(e => e.Parent)
                .Where(e => e.Id != Guid.Empty)
                .OrderByDescending(item => item.IsDirectory)
                .ThenBy(item => item.Name);

            return query.Where(items => items.ParentId == parentKey);
        }
        public async Task<IDictionary<Guid, bool>> GetHasSubDirectoriesInfo(IQueryable<FileItem> fileItems)
        {
            IQueryable<Guid> keys = fileItems
                .Select(e => e.Id);

            return (await FileManagementDbContext
                .FileItems
                .Where(e => e.IsDirectory)
                .Where(e => keys.Contains(e.ParentId))
                .ToListAsync())
                .GroupBy(e => e.ParentId)
                .ToDictionary(group => group.Key, group => group.Any());
        }

        FileItem? GetFileItem(FileSystemItemInfo item)
        {
            var itemId = ParseKey(item.Key);
            return FileManagementDbContext.FileItems.FirstOrDefault(i => i.Id == itemId);
        }

        bool IsFileItemExists(FileSystemItemInfo itemInfo)
        {
            var pathKeys = itemInfo.PathKeys.Select(key => ParseKey(key))
                .ToArray();

            var foundEntries = FileManagementDbContext.FileItems
                .Where(item => pathKeys.Contains(item.Id))
                .Select(item => new { item.Id, item.ParentId, item.Name, item.IsDirectory });

            var pathNames = itemInfo.Path.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            var isDirectoryExists = true;
            for (var i = 0; i < pathKeys.Length && isDirectoryExists; i++)
            {
                var entry = foundEntries.FirstOrDefault(e => e.Id == pathKeys[i]);

                isDirectoryExists = entry != null && entry.Name == pathNames[i] &&
                                    (i == 0 && entry.ParentId == Guid.Empty || entry.ParentId == pathKeys[i - 1]);
                if (entry != null && isDirectoryExists && i < pathKeys.Length - 1)
                    isDirectoryExists = entry.IsDirectory;
            }
            return isDirectoryExists;
        }

        static bool AllowCopyOrMove(FileSystemItemInfo item, FileSystemItemInfo destinationDirectory)
        {
            if (destinationDirectory.PathKeys.Length < item.PathKeys.Length)
                return true;

            var isValid = false;
            for (var i = 0; i < destinationDirectory.PathKeys.Length && !isValid; i++)
            {
                isValid = destinationDirectory.PathKeys[i] != item.PathKeys[i];
            }
            return isValid;
        }

        static FileItem? CreateCopy(FileItem? fileItem)
        {
            if (fileItem == null)
                return null;

            return new FileItem
            {
                Name = fileItem.Name,
                Created = DateTime.Now,
                Modified = DateTime.Now,
                IsDirectory = fileItem.IsDirectory,
                ModifiedById = GuestPersonId
            };
        }

        static Guid ParseKey(string key)
        {
            if (Guid.TryParse(key, out Guid guid))
                return guid;

            return Guid.Empty;
        }

        string GenerateCopiedFileItemName(Guid parentDirKey, string copiedFileItemName, bool isDirectory)
        {
            var dirNames = GetDirectoryContents(parentDirKey)
                .Where(i => i.IsDirectory == isDirectory)
                .Select(i => i.Name);

            string newName;
            var fileExtension = isDirectory ? "" : "." + Path.GetExtension(copiedFileItemName);
            var copyNamePrefix =
                isDirectory ? copiedFileItemName : Path.GetFileNameWithoutExtension(copiedFileItemName);
            var index = -1;
            do
            {
                var pathPostfix = index < 1 ? string.Empty : $" {index}{fileExtension}";
                newName = $"{copyNamePrefix} {(index < 0 ? "" : "copy")}{pathPostfix}";
                index++;
            } while (dirNames.Contains(newName));
            return newName;
        }

        static void ThrowItemNotFoundException(FileSystemItemInfo item)
        {
            var itemType = item.IsDirectory ? "Directory" : "File";
            var errorCode = item.IsDirectory ? FileSystemErrorCode.DirectoryNotFound : FileSystemErrorCode.FileNotFound;
            string message = $"{itemType} '{item.Path}' not found.";
            throw new FileSystemException(errorCode, message);
        }

        static void ThrowNoAccessException()
        {
            string message = "Access denied. The operation cannot be completed.";
            throw new FileSystemException(FileSystemErrorCode.NoAccess, message);
        }

        public void UploadFile(FileSystemUploadFileOptions options)
        {
            var file = new FileItem
            {
                Name = options.FileName,
                Content = new byte[options.FileContent.Length],
                Created = DateTime.Now,
                Modified = DateTime.Now,
                ModifiedById = GuestPersonId,
                ParentId = ParseKey(options.DestinationDirectory.Key)
            };

            options.FileContent.ReadAsync(file.Content, 0, file.Content.Length);

            FileManagementDbContext.FileItems.Add(file);
            FileManagementDbContext.SaveChanges();
        }

        public Stream GetFileContent(FileSystemLoadFileContentOptions options)
        {
            var file = FileManagementDbContext.FileItems.FirstOrDefault(e => e.Id == ParseKey(options.File.Key));

            if (file == null)
                throw new Exception("Not Found");

            return new MemoryStream(file.Content);
        }
    }
}
