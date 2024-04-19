using NUnit.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WordCounter.API.Controllers;
using WordCounter.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace WordCounter.Test;

[TestFixture]
public class WordCounterTest
{
    private WordCounterController _controller;
    private BlobStorageService _blobStorageService;
    private string _testDirectory;

    [SetUp]
    public void SetUp()
    {
        // Configure logging to only include console logging
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        var logger = loggerFactory.CreateLogger<WordCounterController>();

        // Create a test directory for file operations
        _testDirectory = Path.Combine(Path.GetTempPath(), "WordCounterControllerTests");
        Directory.CreateDirectory(_testDirectory);

        // Instantiate BlobStorageService and use the test directory as the working directory
        _blobStorageService = new BlobStorageService();

        // Instantiate the controller with the BlobStorageService
        _controller = new WordCounterController(logger, _blobStorageService);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the test directory
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }

        _controller.Dispose();
    }

    [Test]
    public async Task CountWords_EmptyFile_ReturnsBadRequest()
    {
        // Arrange
        IFormFile file = null;

        // Act: Call CountWords method
        var result = await _controller.CountWords(file);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.NotNull(badRequestResult);
        Assert.AreEqual("File is empty.", badRequestResult.Value);
    }

    [Test]
    public async Task CountWordsAndGetCountResult_SuccessfulFlow()
    {
        // Arrange
        string fileName = "test.txt";
        string fileContent = "do the things, you do so well";

        // Create a MemoryStream and write the file content to it
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        // Reset the position of the stream to the beginning
        memoryStream.Position = 0;

        // Create a FormFile instance and set its ContentType to "text/plain"
        var file = new FormFile(memoryStream, 0, memoryStream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(), // Ensure headers are set (optional if not provided)
            ContentType = "text/plain" // Set the content type explicitly to "text/plain"
        };

        // Act: Call CountWords method
        var countWordsResult = await _controller.CountWords(file);
        Assert.IsInstanceOf<OkObjectResult>(countWordsResult);
        OkObjectResult okResult = countWordsResult as OkObjectResult;

        // Verify that OkObjectResult is not null
        Assert.NotNull(okResult);

        // Retrieve the file URL from the OkObjectResult
        string fileUrl = okResult.Value as string;
        Assert.NotNull(fileUrl);

        // Extract the file name from the URL (assuming the file URL is correctly structured)
        string[] urlParts = fileUrl.Split('/');
        string resultFileName = urlParts[^1];  // Gets the last part of the URL as the file name

        // Act: Call GetCountResult method with the file name obtained from CountWords
        var getCountResult = await _controller.GetCountResult(resultFileName);
        Assert.IsInstanceOf<FileContentResult>(getCountResult);
        FileContentResult fileResult = getCountResult as FileContentResult;

        // Verify that FileContentResult is not null
        Assert.NotNull(fileResult);

        // Check the content type and file download name
        Assert.AreEqual("text/plain", fileResult.ContentType);
        Assert.AreEqual(resultFileName, fileResult.FileDownloadName);

        // Read the file content and check the expected results
        string fileContentRead = Encoding.UTF8.GetString(fileResult.FileContents);
        string expectedFileContent = "do: 2\r\nthe: 1\r\nthings: 1\r\nyou: 1\r\nso: 1\r\nwell: 1\r\n";

        // Verify the content of the file
        Assert.AreEqual(expectedFileContent, fileContentRead);
    }
}