namespace Bot.Gateway.Dto.Responses;

public class FileDetails
{
    public FileDetails(Stream fileContent, string filename, string parentName)
    {
        FileContent = fileContent;
        Filename = filename;
        ParentName = parentName;
    }

    public Stream FileContent { get; set; }
    public string Filename { get; set; }
    public string ParentName { get; set; }
}