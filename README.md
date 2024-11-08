# `Result<T>` Pattern - .NET API

`Result<T>` sýnýfý, özellikle .NET API'lerinde iþlem sonuçlarýný yönetmek için kullanýlan güçlü bir **result pattern** uygulamasýdýr. Bu sýnýf, baþarý durumunda bir deðer (`Value`) ve hata durumunda bir hata mesajý (`Error`) döndürür. Ayrýca, HTTP yanýtlarýný otomatik olarak uygun statülerle (`200 OK` veya `400 BadRequest`) döndürebilmek için `IActionResult`'ý implement eder.

Bu desen, API'lerde hata yönetimi ve baþarýlý sonuçlarý daha tutarlý bir þekilde ele almayý saðlar.

## Özellikler

- **Baþarý Durumu (`IsError`)**: Ýþlemin baþarýlý olup olmadýðýný belirler. Eðer `IsError` `false` ise baþarý, `true` ise hata durumu vardýr.
- **Deðer (`Value`)**: Baþarý durumunda döndürülen deðer.
- **Hata (`Error`)**: Hata durumunda döndürülen hata bilgisi.
- **HTTP Durumu**: Baþarý durumunda `200 OK`, hata durumunda `400 BadRequest` yanýtý döndürülür.
- **JSON Formatý**: Yanýtlar, JSON formatýnda döndürülür.
- **Kolay Kullaným**: `Result<T>` sýnýfý, API yanýtlarýný direkt olarak döndürmenizi saðlar, böylece manuel dönüþüm gerekmez.

## Genel Yapýsý

### 1. `Result<T>` Sýnýfýnýn Yapýsý

`Result<T>` sýnýfý, iþlem sonucunu tutan bir yapýdýr. Baþarý veya hata durumuna göre uygun yanýtý döndüren `ExecuteResultAsync` metodunu içerir.

#### `Result<T>` Sýnýfýnýn Temel Yapýsý

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

    // Baþarý durumunu döndüren metod
    public static Result<T> Success(T value) => new(value);

    // Hata durumunu döndüren metod
    public static Result<T> Failure(Error error) => new(error);

    // IActionResult implementasyonu ile HTTP yanýtlarýný döndüren metod
    public async Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Response.ContentType = "application/json";

        if (!IsError)
        {
            // Baþarý durumu (200 OK)
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

## Örnek Kullanýmý

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