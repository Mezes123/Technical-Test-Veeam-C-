using System;
using System.IO;
using System.Security.AccessControl;

public class Logger
{
    private static readonly object _lock = new object();
    private string _logFilePath;
    

    public Logger(string logFilePath)
    {
        // Check if the provided log file path is actually a directory
        if (Directory.Exists(logFilePath))
        {
            throw new ArgumentException("The log file path provided is a directory, not a file.");
        }

        _logFilePath = logFilePath;

    }

    public void Log(string message)
    {
        // Log the message to the console
        Console.WriteLine(message);

        lock (_lock)
        {
            try
            {
                // Write the log message to a log file
                using (FileStream fs = new FileStream(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.WriteLine(message);
                        writer.Flush();
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to the path: {_logFilePath}. Exception: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"An error occurred while accessing the file: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }
    }


}
