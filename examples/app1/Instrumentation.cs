using System.Diagnostics;

/// <summary>
/// It is recommended to use a custom type to hold references for ActivitySource.
/// This avoids possible type collisions with other components in the DI container.
/// </summary>
public class Instrumentation : IDisposable
{
    internal const string ActivitySourceName = "Application1";

    public Instrumentation()
    {
        this.ActivitySource = new ActivitySource(ActivitySourceName);
    }

    public ActivitySource ActivitySource { get; }

    public void Dispose()
    {
        this.ActivitySource.Dispose();
    }
}