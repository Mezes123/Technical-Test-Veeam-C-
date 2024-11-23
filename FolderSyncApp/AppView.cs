using System;

namespace FolderSyncApp
{
    public class AppView
    {
        private static AppController _controller;
        static string message = "";

        public static void Main(string[] args)
        {
            string folderPath = string.Empty;
            string replicaPath = string.Empty;
            string logFilePath = string.Empty;
            int syncTimeInSeconds;

            bool foundFolderPath = false;

            while (!foundFolderPath)
            {
                Console.Write("Insert the source folder path: ");
                folderPath = Console.ReadLine();
                _controller = new AppController();
                foundFolderPath = _controller.VerifyPath(folderPath);

                if (!foundFolderPath)
                {
                    Console.WriteLine("Invalid Folder path");
                }
            }

            foundFolderPath = false;

            while (!foundFolderPath)
            {
                Console.Write("Insert the replica folder path: ");
                replicaPath = Console.ReadLine();
                foundFolderPath = _controller.VerifyPath(replicaPath);

                if (!foundFolderPath)
                {
                    Console.WriteLine("Invalid Folder path");
                }
            }

            foundFolderPath = false;

            while (!foundFolderPath)
            {
                Console.Write("Insert the full log file path (including file name, e.g., C:\\LogFile\\log.txt): ");
                logFilePath = Console.ReadLine();
                if (File.Exists(logFilePath))
                {
                    foundFolderPath = true;
                }
                else
                {
                    Console.WriteLine("Invalid path");
                }
            }

            // Initialize the final controller with the log file path
            _controller = new AppController(logFilePath);

            // Set up the remaining paths and parameters
            Console.Write("Insert the synchronization interval, in seconds: ");
            syncTimeInSeconds = int.Parse(Console.ReadLine());

            _controller.SetPaths(folderPath, replicaPath, logFilePath);
            _controller.SetSyncTimeInSeconds(syncTimeInSeconds);
            _controller.WatchFiles();



        }
    }
}
