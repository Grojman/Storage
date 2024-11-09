using System.Timers;
public static class AutoSaver
{
    private static Timer timer;
    private static Action elapsed;
    public static void Start()
    {
        if (timer == null)
        {
            timer = new Timer(Settings.Default.AutoSaveTime * 60 * 1000); //Change the auto save time an the static numbers for variables
            timer.AutoReset = true;
            timer.Elapsed += Elapsed;
        }
        timer.Start();
    }

    public static void Stop() => timer.Stop();
    private static void Elapsed(object? _, EventArgs e) => elapsed?.Invoke();

    public static void Subscribe(Action action) => elapsed += action;

    public static void Unsuscribe(Action action) => elapsed -= action;
}