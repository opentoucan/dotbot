using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Dotbot.Gateway.Dto.Responses;

namespace Dotbot.Gateway.Services;

public interface IFileUploadService
{
    Task UploadFile(string parentName, string attachmentName, Stream content, CancellationToken token);
    Task<FileDetails?> GetFile(string parentName, string filename, CancellationToken token);
    Task DeleteFile(string parentName, string attachmentName, CancellationToken token);

}

public class FileUploadService : IFileUploadService
{
    private readonly IAmazonS3 _amazonS3Client;
    private readonly ILogger<FileUploadService> _logger;
    public FileUploadService(IAmazonS3 amazonS3Client, ILogger<FileUploadService> logger)
    {
        _amazonS3Client = amazonS3Client;
        _logger = logger;
    }

    public async Task UploadFile(string parentName, string attachmentName, Stream content, CancellationToken token = default)
    {
        try
        {
            _logger.LogInformation("Saving file {attachment} into bucket {bucket}", attachmentName, parentName);
            using var fileTransferUtility = new TransferUtility(_amazonS3Client);
            var bucketsResponse = await _amazonS3Client.ListBucketsAsync(token);
            if(bucketsResponse.Buckets.FirstOrDefault(bucket => bucket.BucketName == parentName) == null)
                await _amazonS3Client.PutBucketAsync(parentName, token);
            
            var transferRequest = new TransferUtilityUploadRequest
            {
                BucketName = parentName,
                InputStream = content,
                ContentType = MimeTypes.GetMimeType(attachmentName),
                Key = attachmentName,
                DisablePayloadSigning = _amazonS3Client.Config.ServiceURL.StartsWith("https")
            };
            await fileTransferUtility.UploadAsync(transferRequest, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save attachment into bucket");
            throw;
        }
    }
    
    public async Task<FileDetails?> GetFile(string parentName, string filename, CancellationToken token = default)
    {
        try
        {
            var response = await _amazonS3Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = parentName,
                Key = filename
            }, token);

            return new FileDetails(response.ResponseStream, response.Key, response.BucketName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve image");
        }

        return null;
    }

    public async Task DeleteFile(string parentName, string filename, CancellationToken token = default)
    {
        try
        {
            _logger.LogInformation("Deleting file {filename}", filename);
            await _amazonS3Client.DeleteObjectAsync(parentName, filename, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file");
        }
    }
}