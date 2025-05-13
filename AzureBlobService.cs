using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public class AzureBlobService
{
    private readonly string _connectionString;

    public AzureBlobService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultEndpointsProtocol=https;AccountName=storagest10440720;AccountKey=SWZY/tu608dQ9C3tdfruKO9JFWEMWzNeXqvHDgbHbCo36+R4t9wcJWDi+QTPqP/eRffD+3jZfkIW+AStGf8rrw==;EndpointSuffix=core.windows.net");
    }

    public async Task<string> UploadFileAsync(IFormFile file, string containerName)
    {
        var containerClient = new BlobContainerClient(_connectionString, containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var blobClient = containerClient.GetBlobClient(fileName);

        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
        }

        return blobClient.Uri.ToString();
    }

    public async Task<bool> DeleteFileAsync(string fileUrl, string containerName)
    {
        var containerClient = new BlobContainerClient(_connectionString, containerName);
        var fileName = Path.GetFileName(fileUrl);
        var blobClient = containerClient.GetBlobClient(fileName);
        return await blobClient.DeleteIfExistsAsync();
    }
}

