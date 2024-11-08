using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ResultPatternExample.Utils
{
    public sealed class Result<T> : IActionResult
    {
        public T? Value { get; }
        public Error? Error { get; }
        public bool IsError => Error != null;

        private Result(T value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            Error = null;
        }

        private Result(Error error)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error), "Error cannot be null.");
        }

        public static Result<T> Success(T value) => new(value);

        public static Result<T> Failure(Error error) => new(error);

        public async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.ContentType = "application/json";

            if (!IsError)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status200OK;
                if (Value is not null)
                {
                    await context.HttpContext.Response.WriteAsJsonAsync(this, options: new JsonSerializerOptions { PropertyNamingPolicy = null });
                }
            }
            else
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                if (Error is not null)
                {
                    await context.HttpContext.Response.WriteAsJsonAsync(this, options: new JsonSerializerOptions { PropertyNamingPolicy = null });
                }
            }
        }
    }
}
