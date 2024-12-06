[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class TraceAttribute : Attribute
{
  public string OperationName { get; }

    public TraceAttribute(string operationName)
    {
        OperationName = operationName;
    }
}