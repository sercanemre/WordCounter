using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordCounter.Repository;

public class BlobStorageService : IBlobStorageService
{
    private readonly Dictionary<string, string> _storage = new Dictionary<string, string>();

    /// <summary>
    /// Creates a file and saves it in local storage.
    /// As mentioned in TechnicalDesignDocument, this local storage will be replace with Azure blob storage service integration.
    /// </summary>
    /// <param name="fileName">Result file's name.</param>
    /// <param name="content">Result file's content.</param>
    /// <returns>Url to download result file.</returns>
    public async Task<string> SaveFileAsync(string fileName, string content)
    {
        string filePath = CreateFilePath(fileName);

        await File.WriteAllTextAsync(filePath, content);

        return $"https://localhost:7002/wordcounter/getcountresult/{fileName}";
    }

    /// <summary>
    /// Get a result file from local storage.
    /// As mentioned in TechnicalDesignDocument, this local storage will be replace with Azure blob storage service integration.
    /// </summary>
    /// <param name="fileName">Result file's name. Taken from url using RouteAttribute.</param>
    /// <returns>Result file of a previously executed calculation.</returns>
    /// <exception cref="FileNotFoundException">If file doesn't exist.</exception>
    public async Task<string> ReadFileAsync(string fileName)
    {
        string filePath = CreateFilePath(fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found at path: {filePath}");
        }

        // Read the content of the file
        string content = await File.ReadAllTextAsync(filePath);

        return content;
    }

    private static string CreateFilePath(string fileName)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), fileName);
    }

}
