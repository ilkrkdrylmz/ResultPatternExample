namespace ResultPatternExample.Utils
{
    public sealed class CsoftResult<T>
    {
        public T? Value { get; }
        public Error? Error { get; }
        public bool IsError => Error != null;

        private CsoftResult(T value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            Error = null;
        }

        private CsoftResult(Error error) => Error = error ?? throw new ArgumentNullException(nameof(error), "Error cannot be null.");
        public static CsoftResult<T> Success(T value) => new(value);
        public static CsoftResult<T> Failure(Error error) => new(error);
    }
}
