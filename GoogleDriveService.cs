/*
    REQUIRES PACKAGE
    NOT TESTED!!!!!

*/

using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.IO.Packaging;


public static class GoogleDriveService
{
    static DriveService Service { get; set; }
    static string[] Scopes = { DriveService.Scope.Drive };
    static string ApplicationName = Settings.Default.GoogleDriveApplicationName;
    public static bool CanConnectToGoogleDrive { get; set; } = false;

    public static IList<Google.Apis.Drive.v3.Data.File> ListRootDirectories()
    {
        var request = Service.Files.List();
        request.Q = "mimeType='application/vnd.google-apps.folder' and 'root' in parents and trashed=false";
        request.Fields = "nextPageToken, files(id, name)";
        var result = request.Execute();
        IList<Google.Apis.Drive.v3.Data.File> directories = result.Files;

        return directories ?? new List<Google.Apis.Drive.v3.Data.File>();
    }

    public static void TryGetService()
    {
        UserCredential credential;

        try
        {
            using (var stream = new FileStream(Settings.Default.GoogleDriveCredentialsPath, FileMode.Open, FileAccess.Read))
            {
                string credPath = Settings.Default.GoogleDriveTokensPath;
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            Service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var request = Service.About.Get();
            request.Fields = "user";
            var about = request.Execute();


            CanConnectToGoogleDrive = about != null && about.User != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            CanConnectToGoogleDrive = false;
        }
    }

    public static string CreateDirectory(string directoryName)
    {
        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = directoryName,
            MimeType = "application/vnd.google-apps.folder"
        };
        var request = Service.Files.Create(fileMetadata);
        request.Fields = "id";
        var file = request.Execute();
        return file.Id;
    }

    public static string GetFileId(string fileName, string folderId)
    {
        var request = Service.Files.List();
        request.Q = $"name='{fileName}' and trashed=false";

        request.Q += $" and '{folderId}' in parents";
        
        request.Fields = "files(id, name)";
        var result = request.Execute();

        var file = result.Files.FirstOrDefault();
        if (file != null)
        {
            return file.Id;
        }

        return CreateFileInFolder(folderId, fileName);

    }


    public static string CreateFileInFolder(string folderId, string filePath)
    {
        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = Path.GetFileName(filePath),
            Parents = new List<string> { folderId }
        };

        FilesResource.CreateMediaUpload request;
        using (var stream = new FileStream(filePath, FileMode.Open))
        {
            request = Service.Files.Create(fileMetadata, stream, GetMimeType(filePath));
            request.Fields = "id";
            request.Upload();
        }
        var file = request.ResponseBody;
        return file.Id;
    }


    public static void GetFile(string fileName, string folderId, string savePath)
    {
        string fileId = GetFileId(fileName, folderId);

        var request = Service.Files.Get(fileId);
        using (var stream = new FileStream(savePath, FileMode.Create))
        {
            request.Download(stream);
        }
    }

    public static void UpdateFile(string fileName, string folderId, string localFilePath)
    {
        string fileId = GetFileId(fileName, folderId);

        var fileMetadata = new Google.Apis.Drive.v3.Data.File();
        FilesResource.UpdateMediaUpload request;
        using (var stream = new FileStream(localFilePath, FileMode.Open))
        {
            request = Service.Files.Update(fileMetadata, fileId, stream, GetMimeType(localFilePath));
            request.Fields = "id";
            request.Upload();
        }
        var file = request.ResponseBody;
        Console.WriteLine("Updated File ID: " + file.Id);
    }

    private static void DeleteFile(string fileId)
    {
        var request = Service.Files.Delete(fileId);
        request.Execute();
        Console.WriteLine("File deleted: " + fileId);
    }

    private static string GetMimeType(string file) => $"application/{Path.GetExtension(file)}";
}