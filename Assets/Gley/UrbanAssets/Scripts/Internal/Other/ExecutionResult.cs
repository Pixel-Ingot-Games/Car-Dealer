namespace Gley.UrbanSystem.Internal
{
    public readonly struct ExecutionResult<T>
    {
        public T Value { get; }
        public string Error { get; }

        public ExecutionResult(T value, string error)
        {
            Value = value;
            Error = error;
        }
    }
}