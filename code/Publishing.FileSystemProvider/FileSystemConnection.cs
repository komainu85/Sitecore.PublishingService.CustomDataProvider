using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Publishing.Data;

namespace Publishing.FileSystemProvider
{
    public class FileSystemConnection : IFileSystemConnection
    {
        private readonly ILogger<FileSystemConnection> _logger;

        public FileSystemConnection(
            string name,
            DataAccessContextType dataAccessContext,
            ILogger<FileSystemConnection> logger, 
            FileSystemConnectionOptions options)
        {
            Condition.Requires(name, nameof(name)).IsNotNull();
            Condition.Requires(dataAccessContext, nameof(dataAccessContext)).IsNotNull();
            Condition.Requires(logger, nameof(logger)).IsNotNull();
            Condition.Requires(options, nameof(options)).IsNotNull();
            Condition.Requires(options.IdTableConnection, nameof(options.IdTableConnection)).IsNotNull();
            Condition.Requires(options.IdTablePrefix, nameof(options.IdTablePrefix)).IsNotNull();

            Name = name;
            DataAccessContext = dataAccessContext;
            _logger = logger;
            RootFolder = options.RootFolder;
            IdTablePrefix = options.IdTablePrefix;
            IdTableConnectionName = options.IdTableConnection;
        }

        public FileSystemConnection(
            string name,
            DataAccessContextType dataAccessContext,
            ILogger<FileSystemConnection> logger, 
            IConfiguration config)
            : this(name, dataAccessContext, logger, config.As<FileSystemConnectionOptions>())
        {
        }

        public string Name { get; }

        public string RootFolder { get; }

        public DataAccessContextType DataAccessContext { get; }

        public string IdTableConnectionName { get; }

        public string IdTablePrefix { get; set; }

        public Task<bool> Validate(bool throwIfInvalid)
        {
            var isValid = !string.IsNullOrEmpty(RootFolder);
            return Task.FromResult(isValid);
        }

        public void Dispose()
        {

        }
    }
}
