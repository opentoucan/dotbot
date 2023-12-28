using Bot.Gateway.Model.Responses;

namespace Bot.Gateway.Services;

public interface IFileUploadService
{
    Task UploadFile(string parentName, string attachmentName, Stream content);
    Task<FileDetails> GetFile(string parentName, string filename);
}