using System.IO;
using System.Reflection;
using CloudAutomation.Application.Interfaces.File;

namespace CloudAutomation.Application.Implementations.File
{
    public class ReadFile : IReadFile
    {
        private readonly Assembly _assembly;

        public ReadFile()
        {
            _assembly = Assembly.GetExecutingAssembly();
        }

        public string Execute(string assemblyFilePath)
        {
            using var stream = _assembly.GetManifestResourceStream(assemblyFilePath);
            using var reader = new StreamReader(stream!);

            return reader.ReadToEnd();
        }
    }
}