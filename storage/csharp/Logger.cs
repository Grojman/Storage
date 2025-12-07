using Server.Enums;

public static class Logger 
{
    private static readonly object lockObject = new();
    public static string FilePath { get; set; } = $"Log/{DateTime.Now.ToString("yyyy-MM-dd")}_Log.txt";
    public static void Log(LogStatus Status) => Log(Status, "");
    public static void Log(LogStatus Status, string Context) 
    {
        lock(lockObject) 
        {
            if (!Directory.Exists("Log")) Directory.CreateDirectory("Log");

            using(StreamWriter Writter = new(FilePath, File.Exists(FilePath))) 
            {
                Writter.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] : {Status} / {Context}");
            }
        }
    }
}