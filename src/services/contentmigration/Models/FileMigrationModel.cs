
namespace TNO.Services.ContentMigration.Models
{
    /// <summary>
    /// FileMigrationModel class, provides a model for migrating a remote file.
    /// </summary>
    public class FileMigrationModel
    {
        #region Properties
        /// <summary>
        /// get/set - The RSN Id that the file is related to.
        /// </summary>
        public long RSNId { get; set; }

        /// <summary>
        /// get/set - The path to the file.
        /// </summary>
        public string Path { get; set; } = "";

        /// <summary>
        /// get/set - The name of the file.
        /// </summary>
        public string Filename { get; set; } = "";

        /// <summary>
        /// get/set - The content type of the file.
        /// </summary>
        public string ContentType { get; set; } = "";
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of an FileMigrationModel object.
        /// </summary>
        public FileMigrationModel() { }

        /// <summary>
        /// Creates a new instance of an FileMigrationModel object, initializes with specified parameters.
        /// </summary>
        /// <param name="rsnId"></param>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <param name="contentType"></param>
        public FileMigrationModel(long rsnId, string path, string filename, string contentType)
        {
            this.RSNId = rsnId;
            this.Path = path;
            this.Filename = filename;
            this.ContentType = contentType;
        }
        #endregion
    }

}
