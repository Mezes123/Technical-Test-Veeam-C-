using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Timers;

namespace FolderSyncApp
{
    public class AppController
    {
        private string logMessage = "";
        private Logger _logger;
        private static List<string> logLines = new List<string>();
        private System.Timers.Timer syncTimer;
        private static bool changesDetected;
     
        private int syncTimeInSeconds;
        private static readonly object _lock = new object();

        private static AppModel _model;

        // Constructor without parameters
        public AppController()
        {
            _model = new AppModel();
            _logger = new Logger(_model.GetLogFilePath());
           
            
        }

        // Constructor with log file path
        public AppController(string logFilePath)
        {
            _model = new AppModel();
            _logger = new Logger(logFilePath);
           
            
        }

        // Verifies if the provided path is valid
        public bool VerifyPath(string path)
        {
            if (path == "")
            {
                return false;
            }

            try
            {
                string directory = Path.GetDirectoryName(path);
                string filename = Path.GetFileName(path);
                if (Directory.Exists(directory) && !string.IsNullOrEmpty(filename))
                {
                    return true;
                }
            }
            catch
            {
                // Ignore exception and return false
            }
            return false;
        }

        // Sets the paths for source folder, replica folder, and log file
        public void SetPaths(string folderPath, string replicaPath, string logFilePath)
        {
            if (!IsDirectoryWritable(Path.GetDirectoryName(logFilePath)))
            {
                return;
            }

            _model.SetPaths(folderPath, replicaPath, logFilePath);
        }

        // Sets the synchronization interval in seconds
        public void SetSyncTimeInSeconds(int syncTimeInSeconds)
        {
            if (syncTimeInSeconds <= 0)
            {
                throw new ArgumentException("Sync time must be greater than zero.");
            }

            _model.SetSyncTimeInSeconds(syncTimeInSeconds);

            // Configure and start the timer for synchronization
            syncTimer = new System.Timers.Timer
            {
                Interval = syncTimeInSeconds * 1000,
                AutoReset = true,
                Enabled = true
            };
            syncTimer.Elapsed += OnTimedEvent;

        }

        // Starts watching for file changes in the source folder
        public void WatchFiles()
        {
            string folderPath = _model.GetFolderPath();
            FileSystemWatcher watcher = new FileSystemWatcher(folderPath)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName
            };

            // Attach event handlers for file changes
            watcher.Created += OnCreated;
            watcher.Changed += OnChanged;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;

            watcher.EnableRaisingEvents = true;



            Console.WriteLine($"Monitoring changes in folder: {folderPath}");
            Console.WriteLine("Press [Enter] to exit...");
            Console.ReadLine();
        }

        // Event handler for changed files
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            FileSystemWatcher watcher = (FileSystemWatcher)sender;
            watcher.EnableRaisingEvents = false;

         
            changesDetected = true;
            LogEvent($"{DateTime.Now}: {e.ChangeType} - {e.FullPath}");

            watcher.EnableRaisingEvents = true;
        }


        // Event handler for created files
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
          
            changesDetected = true;
            LogEvent($"{DateTime.Now}: {e.ChangeType} - {e.FullPath}");
        }

        // Event handler for deleted files
        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
     
            changesDetected = true;
            LogEvent($"{DateTime.Now}: {e.ChangeType} - {e.FullPath}");
        }

        // Event handler for renamed files
        private void OnRenamed(object sender, RenamedEventArgs e)
        {
                      changesDetected = true;
            LogEvent($"{DateTime.Now}: Renamed - From: {e.OldFullPath} To: {e.FullPath}");
        }

        // Timer event handler for periodic synchronization
        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
                _model.SyncDirectories(_model.GetFolderPath(), _model.GetReplicaPath());
                Console.WriteLine($"{DateTime.Now}: Synchronizing changes...");
                changesDetected = false;
                Console.WriteLine($"{DateTime.Now}: Synchronization complete.");
               
        }

        // Logs an event message
        private void LogEvent(string message)
        {
            _logger.Log(message);
        }

        // Checks if the directory is writable
        private bool IsDirectoryWritable(string dirPath)
        {
            try
            {
                string tempFilePath = Path.Combine(dirPath, "tempfile.txt");
                File.WriteAllText(tempFilePath, "test");
                File.Delete(tempFilePath);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
