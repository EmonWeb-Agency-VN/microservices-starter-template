using System.Reflection;
using Common.SharedKernel.LogProvider;
using NLog;

namespace Common.SharedKernel.Utilities
{
    public class FileReaderUtils
    {
        private static readonly Logger _logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static byte[]? ReadJsonFiles(string fileName)
        {
            try
            {
                _logger.Info($"Start reading files. File name: {fileName}");
                var currentDirectory = Directory.GetCurrentDirectory();
                var parentDirectory = Directory.GetParent(currentDirectory);
                if (parentDirectory == null)
                {
                    return null;
                }
                var path = Path.Combine(parentDirectory.ToString(), "Common", "Common.Persistence", "SeedData", fileName);
                using var fs = new FileStream(path, FileMode.Open);
                using BinaryReader br = new(fs);
                byte[] buffer = [];
                br.BaseStream.Seek(0, SeekOrigin.Begin);
                buffer = br.ReadBytes((int)br.BaseStream.Length);
                _logger.Info($"Finish reading files. File name: {fileName}");
                return buffer;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error reading files. File name: {fileName}. Message: {ex.Message}", ex);
                return null;
            }
        }
    }
}
