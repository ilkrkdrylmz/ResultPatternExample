# `Result<T>` Pattern - .NET API

`Result<T>` s�n�f�, �zellikle .NET API'lerinde i�lem sonu�lar�n� y�netmek i�in kullan�lan g��l� bir **result pattern** uygulamas�d�r. Bu s�n�f, ba�ar� durumunda bir de�er (`Value`) ve hata durumunda bir hata mesaj� (`Error`) d�nd�r�r. Ayr�ca, HTTP yan�tlar�n� otomatik olarak uygun stat�lerle (`200 OK` veya `400 BadRequest`) d�nd�rebilmek i�in `IActionResult`'� implement eder.

Bu desen, API'lerde hata y�netimi ve ba�ar�l� sonu�lar� daha tutarl� bir �ekilde ele almay� sa�lar.

## �zellikler

- **Ba�ar� Durumu (`IsError`)**: ��lemin ba�ar�l� olup olmad���n� belirler. E�er `IsError` `false` ise ba�ar�, `true` ise hata durumu vard�r.
- **De�er (`Value`)**: Ba�ar� durumunda d�nd�r�len de�er.
- **Hata (`Error`)**: Hata durumunda d�nd�r�len hata bilgisi.
- **HTTP Durumu**: Ba�ar� durumunda `200 OK`, hata durumunda `400 BadRequest` yan�t� d�nd�r�l�r.
- **JSON Format�**: Yan�tlar, JSON format�nda d�nd�r�l�r.
- **Kolay Kullan�m**: `Result<T>` s�n�f�, API yan�tlar�n� direkt olarak d�nd�rmenizi sa�lar, b�ylece manuel d�n���m gerekmez.

## Genel Yap�s�

### 1. `Result<T>` S�n�f�n�n Yap�s�

`Result<T>` s�n�f�, i�lem sonucunu tutan bir yap�d�r. Ba�ar� veya hata durumuna g�re uygun yan�t� d�nd�ren `ExecuteResultAsync` metodunu i�erir.

#### `Result<T>` S�n�f�n�n Temel Yap�s�

```csharp
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

    // Ba�ar� durumunu d�nd�ren metod
    public static Result<T> Success(T value) => new(value);

    // Hata durumunu d�nd�ren metod
    public static Result<T> Failure(Error error) => new(error);

    // IActionResult implementasyonu ile HTTP yan�tlar�n� d�nd�ren metod
    public async Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Response.ContentType = "application/json";

        if (!IsError)
        {
            // Ba�ar� durumu (200 OK)
            context.HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            if (Value is not null)
            {
                await context.HttpContext.Response.WriteAsJsonAsync(this, options: new JsonSerializerOptions { PropertyNamingPolicy = null });
            }
        }
        else
        {
            // Hata durumu (400 BadRequest)
            context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            if (Error is not null)
            {
                await context.HttpContext.Response.WriteAsJsonAsync(this, options: new JsonSerializerOptions { PropertyNamingPolicy = null });
            }
        }
    }
}
```

## �rnek Kullan�m�

```csharp
[HttpGet(Name = "GetWeatherForecast")]
public IActionResult Get()
{
    try
    {
        var response = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        });
        return Result<IEnumerable<WeatherForecast>>.Success(response);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "WeatherForecast General Error");
        return Result<IEnumerable<WeatherForecast>>.Failure(new Error(999, ex.Message));
    }
}
```