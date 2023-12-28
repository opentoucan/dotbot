using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Bot.Gateway.Model.Responses;

namespace Bot.Gateway.Services;

public class FileUploadService : IFileUploadService
{
    private readonly IAmazonS3 _amazonS3Client;

    public FileUploadService(IAmazonS3 amazonS3Client)
    {
        _amazonS3Client = amazonS3Client;
    }

    public async Task UploadFile(string parentName, string attachmentName, Stream content)
    {
        using var fileTransferUtility = new TransferUtility(_amazonS3Client);
        var transferRequest = new TransferUtilityUploadRequest
        {
            BucketName = parentName,
            InputStream = content,
            ContentType = MimeTypes.GetMimeType(attachmentName),
            Key = attachmentName
        };
        await fileTransferUtility.UploadAsync(transferRequest);
    }
    
    public async Task<FileDetails> GetFile(string parentName, string filename)
    {
        var response = await _amazonS3Client.GetObjectAsync(new GetObjectRequest
        {
            BucketName = parentName,
            Key = filename
        });
        
        return new FileDetails(response.ResponseStream, response.Key, response.BucketName);
    }
}