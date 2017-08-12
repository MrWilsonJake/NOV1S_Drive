using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoogleFile = Google.Apis.Drive.v3.Data.File;

namespace NOV1S_Drive
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        private static string[] Scopes = { DriveService.Scope.DriveReadonly };
        private static string ApplicationName = "NOV1S_Drive";

        /// <summary>
        /// Singleton Credential
        /// </summary>
        private static UserCredential _credential;
        /// <summary>
        /// Create Singleton Credential
        /// </summary>
        private static UserCredential Credential
        {
            get
            {
                if (_credential == null)
                {
                    using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                    {
                        string credPath = System.Environment.GetFolderPath(
                            System.Environment.SpecialFolder.Personal);
                        credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                        Console.WriteLine("Credential file saved to: " + credPath);

                        _credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.Load(stream).Secrets,
                            Scopes,
                            "user",
                            CancellationToken.None,
                            new FileDataStore(credPath, true)).Result;
                    }
                }
                return _credential;
            }
        }

        /// <summary>
        /// Singleton DriveService
        /// </summary>
        private static DriveService _service;

        /// <summary>
        /// Create Singleton Drive API service.
        /// </summary>
        private static DriveService Service
        {
            get
            {
                if (_service == null)
                {
                    _service = new DriveService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = Credential,
                        ApplicationName = ApplicationName,
                    });
                }
                return _service;
            }
        }

        private static Dictionary<string, GoogleFile> FolderLookup;


        static void Main(string[] args)
        {
            var t = Task.Run(() => CreateFileWatcher(@"C:\Temp"));
            t.Wait();
            FolderLookup = GetFolders();


            

            GoogleFile temp = FolderLookup.Where(a => a.Value.Name == "My Drive").FirstOrDefault().Value;
            GoogleFile temp0 = FolderLookup.Where(a => a.Value.Id == "0AGRY-dljjiSAUk9PVA").FirstOrDefault().Value;

            //Console.ReadLine();
            GetFolderPaths();
            //Console.ReadLine();

            GetFiles();
            GoogleFile temp1 = FolderLookup.Where(a => a.Value.Id == "0AGRY-dljjiSAUk9PVA").FirstOrDefault().Value;
            Console.ReadLine();

        }
        

        private static Dictionary<string, GoogleFile> GetFolders()
        {
            Dictionary<string, GoogleFile> folderList = new Dictionary<string, GoogleFile>();
            int counter = 0;

            try
            {
                string pageToken = null;
                do
                {
                    var request = Service.Files.List();
                    request.Q = "mimeType='application/vnd.google-apps.folder'";
                    request.Spaces = "drive";
                    request.PageToken = pageToken;
                    request.Fields = "*";
                    var result = request.Execute();

                    foreach (var file in result.Files)
                    {
                        folderList.Add(file.Id, file);
                        //Console.WriteLine("{0}. {1} ({2})", counter, file.Name, file.Id);
                        counter++;
                    }
                    pageToken = result.NextPageToken;

                } while (pageToken != null);
            }
            catch (Exception ex)
            {
                Console.Write($"Error getting Files: {ex.Message}");
            }

            Console.WriteLine($"Folders Found: {counter}");

            return folderList;
        }


        private static void GetFolderPaths()
        {
            GoogleFile temp2 = Service.Files.Get("root").Execute();

            //Dictionary<string, GoogleFile> rootFolders = FolderLookup.Where(p => p.Value.Parents)

        }



        private static IList<GoogleFile> GetFiles()
        {
            IList<GoogleFile> fileList = new List<GoogleFile>();

            try
            {
                int counter = 0;
                string pageToken = null;
                do
                {
                    var request = Service.Files.List();
                    request.Q = "mimeType!='application/vnd.google-apps.folder'";
                    request.Spaces = "drive";
                    request.PageToken = pageToken;
                    request.Fields = "*";
                    var result = request.Execute();

                    foreach (var file in result.Files)
                    {
                        Console.WriteLine("{0}. {1} ({2})", counter, file.Name, file.Id);
                        counter++;
                    }
                    pageToken = result.NextPageToken;

                } while (pageToken != null);
            }
            catch (Exception ex)
            {
                Console.Write($"Error getting Files: {ex.Message}");
            }

            return fileList;
        }



        public static void CreateFileWatcher(string path)
        {
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Filter = "*.txt";
            
         

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        // Define the event handlers.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }




    }
}
