# Word Counter Web Application

## Assignment Overview

The task is to construct a web application in .NET 8 that counts the number of occurrences of words in a file and outputs the results. This application should be designed as a cloud-based web application to match the focus on cloud development.

## Approach and Implementation

### Application Design

1. **API Structure**:
    - The application is structured as an API with two endpoints:
        - `/WordCounter/CountWords`: Accepts a file, processes it, and returns the URL of the file with the word count results.
        - `/WordCounter/GetCountResult/{fileName}`: Accepts a file name and returns the file with the word count results.

2. **Controller**:
    - The `WordCounterController` handles both endpoints and orchestrates the process flow.
        - The `CountWords` method receives a file, counts the words, saves the word count results to a file, and returns the file's URL.
        - The `GetCountResult` method accepts a file name, reads the file from storage, and returns the file content.

3. **Storage**:
    - `BlobStorageService` handles file storage and retrieval using the local file system.
        - This service saves the word count results as files and reads files when requested.
        - In a production environment, this can be adapted to use cloud storage like Azure Blob Storage.

### Cloud Considerations

- **Deployment**:
    - Application is containerized using Docker for easy scaling and management.
    - Automate deployment pipelines with CI/CD tools in Azure DevOps.

- **Storage**:
    - Replace local file storage with Azure Blob Storage service for production. This provides better scalability and reliability.

### Testing

1. **Unit Tests**:
    - Unit tests are written using NUnit to validate the functionality of the `WordCounterController`.
    - Tests cover the entire process flow, starting with the `CountWords` method and ending with the `GetCountResult` method.
    - The tests verify:
        - Correct responses from the API methods.
        - Accurate word counts in the output file.
        - The successful flow of processing the input file and retrieving the output file.

2. **Testing Strategy**:
    - The testing strategy is based on testing the main flow of the application: from processing the input file to returning the word count results.
    - The unit tests ensure the correct behavior of the `WordCounterController` and the file storage operations.

## Summary

- The application is designed as a web API with two endpoints for processing files and retrieving word count results.
- The application utilizes the local file system for storage and retrieval in the assignment, but it can be adapted to use a cloud storage service in production.
- The approach ensures modular design and separation of concerns, making it easier to adapt and scale the application in a cloud environment.
- Unit tests validate the application flow and check the results of the file processing and retrieval operations.