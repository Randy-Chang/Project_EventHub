namespace EventHub.Display;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new DisplayForm(DisplayClientOptions.Load()));
    }
}
