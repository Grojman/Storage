using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class FileTracker
{
    private static FileSystemWatcher watcher = new();

    public static void Start(Action<object, FileSystemEventArgs> action)
    {
        UpdatePath();
        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        watcher.Changed += new FileSystemEventHandler(action);
        watcher.Deleted += new FileSystemEventHandler(action);
        watcher.Renamed += new RenamedEventHandler(action);
        watcher.Created += new FileSystemEventHandler(action);
        watcher.EnableRaisingEvents = true;
    }

    public static void UpdatePath()
    {
        if (!watcher.Path.Equals(Settings.Default.TextsFolderPath))
        {
            watcher.EnableRaisingEvents = false;
            watcher.Path = Settings.Default.TextsFolderPath;
            watcher.EnableRaisingEvents = true;
        }
    }
}