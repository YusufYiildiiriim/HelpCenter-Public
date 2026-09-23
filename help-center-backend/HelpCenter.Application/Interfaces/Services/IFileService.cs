using HelpCenter.Application.Common.Models;

namespace HelpCenter.Application.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Saves the file to the specified folder.
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="folderPath">Folder path under Uploads (e.g. "MessageDocuments/1/TKT-123")</param>
        /// <returns>Web access path of the saved file</returns>
        Task<string> SaveFileAsync(FileUpload file, string folderPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves the file with the given name (extension preserved) to the specified folder.
        /// For scenarios requiring sequential naming (e.g. message attachments 1.png, 2.pdf).
        /// </summary>
        /// <returns>Web access path of the saved file</returns>
        Task<string> SaveFileAsAsync(FileUpload file, string folderPath, string fileNameWithoutExtension, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the file at the specified path.
        /// </summary>
        /// <param name="path">Web path or full path of the file</param>
        void DeleteFile(string path);

        /// <summary>
        /// Checks whether the folder exists and creates it if it does not.
        /// </summary>
        /// <param name="folderPath">Folder path under Uploads</param>
        void EnsureDirectoryExists(string folderPath);
    }
}
