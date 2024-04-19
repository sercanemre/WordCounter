using Microsoft.AspNetCore.Mvc;
using System.Text;
using WordCounter.Repository;

namespace WordCounter.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class WordCounterController : Controller
{
    private readonly ILogger<WordCounterController> _logger;

    private readonly IBlobStorageService _blobStorageService;

    public WordCounterController(ILogger<WordCounterController> loggerParam, IBlobStorageService blobStorageParam)
    {
        this._logger = loggerParam;
        this._blobStorageService = blobStorageParam;
    }

    /// <summary>
    /// Counts words in a file. Result is stored as a file.
    /// </summary>
    /// <param name="file">File that its words will be count.</param>
    /// <returns>Url to download result file.</returns>
    [HttpPost]
    public async Task<IActionResult> CountWords([FromBody] IFormFile file)
    {
        // Validate input
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        // Validate file content type
        if (file.ContentType != "text/plain")
        {
            return BadRequest("Only plain text files are allowed.");
        }

        try
        {
            // Call a private method to count words. Details of this operation are explained within the method.
            Dictionary<string, int> wordDictionary = await CountWordsAsync(file);

            // Create a result file in containing words and their counts.
            string fileContent = WriteWordCountsToFileAsync(wordDictionary);

            // Call blob storage service to create a file containing the result.
            // Add DateTime.Now.Ticks to provided file name and set it as the result file's name.
            string resultFileUrl = await this._blobStorageService.SaveFileAsync(file.FileName + "_" + DateTime.Now.Ticks, fileContent);

            // Return a Url that download the result file.
            return Ok(resultFileUrl);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex.Message, ex);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<Dictionary<string, int>> CountWordsAsync(IFormFile file)
    {
        Dictionary<string, int> wordDictionary = new();

        // Create a stream reader to read the file line by line
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                // Split word by space or punctuation
                var words = line.Split(new[] { ' ', ',', '.', ';', ':', '!', '?' },
                                        StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    // Call ToLower and Trim extensions on word instance to avoid creating duplicate record for a single word.
                    var normalizedWord = word.ToLowerInvariant().Trim();

                    // Check if word was already encountered. If so increase its encoutered count, otherwise create a new Dictionary instance for newly encountered word.
                    if (wordDictionary.ContainsKey(normalizedWord))
                    {
                        wordDictionary[normalizedWord]++;
                    }
                    else
                    {
                        wordDictionary[normalizedWord] = 1;
                    }
                }
            }
        }

        return wordDictionary;
    }

    private string WriteWordCountsToFileAsync(Dictionary<string, int> wordCounts)
    {
        // Create a string builder instance and iterate through the dictionary to fill result file.
        var sb = new StringBuilder();
        foreach (var entry in wordCounts)
        {
            sb.AppendLine($"{entry.Key}: {entry.Value}");
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Returns the desired result file.
    /// </summary>
    /// <param name="fileName">Result file name. This name is generated in CountWords method in this Controller.</param>
    /// <returns>Result file that is containing word cunt results.</returns>
    [Route("[controller]/[action]/{fileName}")]
    [HttpPost]
    public async Task<IActionResult> GetCountResult(string fileName)
    {
        // Validate input
        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest("File name is required.");
        }

        try
        {
            // Read the content of the file from filePath
            string content = await _blobStorageService.ReadFileAsync(fileName);

            // Convert string content to a byte array
            byte[] fileBytes = Encoding.UTF8.GetBytes(content);

            // Determine the file type (e.g., plain text)
            string fileType = "text/plain";

            // Return the file as a FileContentResult
            return File(fileBytes, fileType, fileName);
        }
        catch (FileNotFoundException fileNotFoundEx)
        {
            this._logger.LogError(fileNotFoundEx.Message, fileNotFoundEx);
            return StatusCode(StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex.Message, ex);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
