using GeneradorInformesCualitativos.Models;
using GeneradorInformesCualitativos.Properties;
using RtfPipe.Tokens;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace GeneradorInformesCualitativos.Services;

public static class LocalBackupHandler
{
    public static ObservableCollection<Backup> Backups { get; set; } = new();
    
    private static void CleanOldBackups()
    {
        var backupFiles = new DirectoryInfo(ApplicationPaths.backupDirectory).GetDirectories()
                           .OrderBy(f => f.CreationTime)
                           .ToList();

        while (backupFiles.Count > Settings.Default.MaxAmountOfBackups)
        {
            var oldestBackup = backupFiles.First();
            oldestBackup.Delete();
            backupFiles.RemoveAt(0);
            Backups.Remove(Backups.OrderBy(n => n.Date).First());
        }
    }

    public static void CreateBackup()
    {
        string backupName = $"backup_{DateTime.Now:yyyyMMdd_HHmm}";

        if (Directory.Exists(ApplicationPaths.root) && Directory.Exists(Path.Join(ApplicationPaths.backupDirectory, backupName)));
        {
            CleanOldBackups();

            if (!Directory.Exists(ApplicationPaths.backupDirectory)) Directory.CreateDirectory(ApplicationPaths.backupDirectory);
            
            DirectoryInfo source = new DirectoryInfo(ApplicationPaths.root);

            DirectoryInfo target = Directory.CreateDirectory(Path.Join(ApplicationPaths.backupDirectory, backupName));

            CopyFilesRecursively(source, target, false);

            Backups.Add(new(ParseDateTime(backupName)));
        }
    }

    private static DateTime ParseDateTime(string dirName) => DateTime.ParseExact(dirName.Substring(7), "yyyyMMdd_HHmm", null, System.Globalization.DateTimeStyles.None);
    private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target, bool overwritte)
    {
        foreach (DirectoryInfo dir in source.GetDirectories())
            CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name), overwritte);
        foreach (FileInfo file in source.GetFiles())
            file.CopyTo(Path.Combine(target.FullName, file.Name), overwritte);
    }

    public static bool LoadBackup(Backup backup)
    {
        string directoryName = $"backup_{backup.Date:yyyyMMdd_HHmm}";
        DirectoryInfo source = new(Path.Join(ApplicationPaths.backupDirectory, directoryName));
        DirectoryInfo target = new(ApplicationPaths.root);

        if(source.Exists && target.Exists)
        {
            CopyFilesRecursively(source, target, true);

            return true;
        }
        return false;
    }

    public static void LoadBackups()
    {
        if (Directory.Exists(ApplicationPaths.backupDirectory))
        {
            DirectoryInfo info = new(ApplicationPaths.backupDirectory);

            foreach(DirectoryInfo dif in info.GetDirectories())
            {
                Backups.Add(new(ParseDateTime(dif.Name)));
            }
        }
    }
}