using System;
using System.IO;
using System.Threading;

namespace FolderSyncApp
{
    public class AppModel
    {
        private string logFilePath;
        private string folderPath;
        private string replicaPath;
        private int syncTimeInSeconds;
        private string[] sourceFiles;
        private string[] replicaFiles;

        public AppModel()
        {
            this.syncTimeInSeconds = 0;
            this.logFilePath = string.Empty;
            this.replicaPath = string.Empty;
            this.folderPath = string.Empty;
            this.sourceFiles = Array.Empty<string>();
            this.replicaFiles = Array.Empty<string>();
        }

        // Sets the paths for the source folder, replica folder, and log file
        public void SetPaths(string folderPath, string replicaPath, string logFilePath)
        {
            this.folderPath = folderPath;
            this.replicaPath = replicaPath;
            this.logFilePath = logFilePath;
            this.sourceFiles = Directory.GetFiles(folderPath);
            this.replicaFiles = Directory.GetFiles(replicaPath);
        }

        // Sets the synchronization interval in seconds
        public void SetSyncTimeInSeconds(int syncTimeInSeconds)
        {
            this.syncTimeInSeconds = syncTimeInSeconds;
        }

        // Gets the path of the source folder
        public string GetFolderPath()
        {
            return this.folderPath;
        }

        // Gets the path of the replica folder
        public string GetReplicaPath()
        {
            return this.replicaPath;
        }

        // Gets the path of the log file
        public string GetLogFilePath()
        {
            return this.logFilePath;
        }

        // Verifies if the directory exists
        public bool VerifyPath(string path)
        {
            return Directory.Exists(path);
        }

        // Gets the relative path for the file in the replica folder
        private string GetFilePaths(string sourceFilePath)
        {
            string relativePath = Path.GetRelativePath(folderPath, sourceFilePath);
            return Path.Combine(replicaPath, relativePath);
        }

        // Copies a file from the source folder to the replica folder
        public void FileCopyToReplica(string sourceFile)
        {
            string replicaPathFile = GetFilePaths(sourceFile);

            bool isFileReady = false;
            int maxAttempts = 10;
            int attempt = 0;

            while (!isFileReady && attempt < maxAttempts)
            {
                try
                {
                    using (FileStream stream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        isFileReady = true;
                    }
                }
                catch (IOException)
                {
                    attempt++;
                    Thread.Sleep(100); // Wait a small period of time before trying again
                }
            }

            if (isFileReady)
            {
                File.Copy(sourceFile, replicaPathFile, true);
            }
            else
            {
                Console.WriteLine($"Failed to copy {sourceFile} to {replicaPathFile} after {maxAttempts} attempts.");
            }
        }

        // Deletes a file from the replica folder
        public void FileDeleteFromReplica(string sourceFile)
        {
            string replicaPathFile = GetFilePaths(sourceFile);
            File.Delete(replicaPathFile);
        }

        // Synchronizes the source and replica directories
        public void SyncDirectories(string source, string target)
        {
            var sourceDir = new DirectoryInfo(source);
            var targetDir = new DirectoryInfo(target);

            // Copy files from source to target if they are newer or missing
            foreach (var sourceFile in sourceDir.GetFiles())
            {
                string targetFilePath = Path.Combine(target, sourceFile.Name);
                if (!File.Exists(targetFilePath) || sourceFile.LastWriteTime > new FileInfo(targetFilePath).LastWriteTime)
                {
                    FileCopyToReplica(sourceFile.FullName);
                }
            }

            // Recursively synchronize subdirectories
            foreach (var sourceSubDir in sourceDir.GetDirectories())
            {
                string targetSubDirPath = Path.Combine(target, sourceSubDir.Name);
                if (!Directory.Exists(targetSubDirPath))
                {
                    Directory.CreateDirectory(targetSubDirPath);
                }
                SyncDirectories(sourceSubDir.FullName, targetSubDirPath);
            }

            // Delete files from target if they no longer exist in source
            foreach (var targetFile in targetDir.GetFiles())
            {
                string sourceFilePath = Path.Combine(source, targetFile.Name);
                if (!File.Exists(sourceFilePath))
                {
                    FileDeleteFromReplica(targetFile.FullName);
                }
            }

            // Delete subdirectories from target if they no longer exist in source
            foreach (var targetSubDir in targetDir.GetDirectories())
            {
                string sourceSubDirPath = Path.Combine(source, targetSubDir.Name);
                if (!Directory.Exists(sourceSubDirPath))
                {
                    Directory.Delete(targetSubDir.FullName, true);
                }
            }
        }
    }
}

