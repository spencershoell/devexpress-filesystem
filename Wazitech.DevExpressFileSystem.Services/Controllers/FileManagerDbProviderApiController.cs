using DevExtreme.AspNet.Mvc.FileManagement;
using Microsoft.AspNetCore.Mvc;

namespace Wazitech.DevExpressFileSystem.Services
{
    public class FileManagerDbProviderApiController : Controller
    {
        public FileManagerDbProviderApiController(DbFileProvider dbFileProvider)
        {
            DBFileProvider = dbFileProvider ?? throw new ArgumentNullException(nameof(dbFileProvider));
        }

        protected DbFileProvider DBFileProvider { get; }

        [Route("api/file-manager-db", Name = "FileManagerDBProviderApi")]
        public object Process(FileSystemCommand command, string arguments)
        {
            var config = new FileSystemConfiguration
            {
                Request = Request,
                FileSystemProvider = DBFileProvider,
                AllowCopy = true,
                AllowCreate = true,
                AllowMove = true,
                AllowDelete = true,
                AllowRename = true,
                AllowedFileExtensions = Array.Empty<string>(),
                AllowDownload = true,
                AllowUpload = true
            };
            var processor = new FileSystemCommandProcessor(config);
            var result = processor.Execute(command, arguments);

            if (result != null)
            {
                //if (command == FileSystemCommand.Download)
                //    return result.GetClientCommandResult();

                return Ok(result.GetClientCommandResult());
            }

            return BadRequest();
        }
    }
}