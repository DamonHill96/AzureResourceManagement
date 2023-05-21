using Azure;
using Azure.Core;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureResourceManagement.Contexts;
using static AzureResourceManagement.Examples.Create.Constants;

namespace AzureResourceManagement.Examples.Create.Definitions.Storage;

public class Storage : AzureResourceDefinition
{
    public Storage(ResourceContext resourceContext) : base(resourceContext)
    {
    }

    protected override async Task<ResourceIdentifier> Create()
    {
        var parameters = new StorageAccountCreateOrUpdateContent(
            new StorageSku(StorageSkuName.StandardLRS), StorageKind.Storage, AzureLocation.UKSouth);
        var accountCollection = ResourceContext.ResourceGroup.GetStorageAccounts();
        var account = (await accountCollection.CreateOrUpdateAsync(WaitUntil.Completed, "example-app", parameters)).Value;

        OutputToApplication(OutputKeys.StorageAccountHostName, account.Data.PrimaryEndpoints.Blob.Remove(account.Data.PrimaryEndpoints.Blob.Length - 1).Replace("https://", ""));
        return account.Id;
    }

    protected override async Task<bool> PostCreate(ResourceGroupResource resourceGroup, ResourceIdentifier createdResource)
    {
        Console.WriteLine("Executing Post Create step: Upload UI files.");

        var distFiles = Directory.GetFiles(ResourceContext.GetVariable("UIFilesPath"));

        var connString = await Helpers.GetStorageAccountConnectionStringAsync(resourceGroup, createdResource);
        
        var blobContainerClient = new BlobContainerClient(connString, StorageContainerName);
        await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var uploadTasks = distFiles.Select(async distFile =>
        {
            var fileName = GetFileNameForUpload();
            Console.WriteLine($"Uploading File: {fileName}");

            var blobClient = new BlobClient(connString, StorageContainerName, fileName);

            await blobClient.UploadAsync(distFile, overwrite: true);

            if (Path.GetExtension(fileName) == ".css")
            {
                await SetCssContentType();
            }
            
            string GetFileNameForUpload()
            {
                var filename = Path.GetFileName(distFile);

                // rename the files
                return filename switch
                {
                    { } main when main.Contains("main") => "main.js",
                    { } polyfills when polyfills.Contains("polyfills") => "polyfills.js",
                    { } runtime when runtime.Contains("runtime") => "runtime.js",
                    { } vendor when vendor.Contains("vendor") => "vendor.js",
                    { } styles when styles.Contains("styles") => "styles.css",
                    _ => filename
                };
            }

            async Task SetCssContentType()
            {
                BlobProperties properties = await blobClient.GetPropertiesAsync();

                var headers = new BlobHttpHeaders
                {
                    ContentType = "text/css",
                    ContentLanguage = properties.ContentLanguage,
                    CacheControl = properties.CacheControl,
                    ContentDisposition = properties.ContentDisposition,
                    ContentEncoding = properties.ContentEncoding,
                    ContentHash = properties.ContentHash
                };

                // Set the blob's properties.
                await blobClient.SetHttpHeadersAsync(headers);
            }
        });

        await Task.WhenAll(uploadTasks);

        return true;
    }
}