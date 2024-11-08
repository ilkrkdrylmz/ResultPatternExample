namespace ResultPatternExample.Utils
{
    public sealed record Error(int Code, string Description)
    {
        public static Error ProductNotFound => new(100, "Product not found");

        public static Error ProductBadRequest => new(101, "Product bad request");
    }
}
