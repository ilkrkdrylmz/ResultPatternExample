# `Result<T>` Pattern - .NET API için

`Result<T>` sınıfı, özellikle .NET API'lerinde işlem sonuçlarını yönetmek için kullanılan güçlü bir **result pattern** uygulamasıdır. Bu sınıf, başarı durumunda bir değer (`Value`) ve hata durumunda bir hata mesajı (`Error`) döndürür. Ayrıca, HTTP yanıtlarını otomatik olarak uygun statülerle (`200 OK` veya `400 BadRequest`) döndürebilmek için `IActionResult`'ı implement eder.

Bu desen, API'lerde hata yönetimi ve başarılı sonuçları daha tutarlı bir şekilde ele almayı sağlar.

## Özellikler

- **Başarı Durumu (`IsError`)**: İşlemin başarılı olup olmadığını belirler. Eğer `IsError` `false` ise başarı, `true` ise hata durumu vardır.
- **Değer (`Value`)**: Başarı durumunda döndürülen değer.
- **Hata (`Error`)**: Hata durumunda döndürülen hata bilgisi.
- **HTTP Durumu**: Başarı durumunda `200 OK`, hata durumunda `400 BadRequest` yanıtı döndürülür.
- **JSON Formatı**: Yanıtlar, JSON formatında döndürülür.
- **Kolay Kullanım**: `Result<T>` sınıfı, API yanıtlarını direkt olarak döndürmenizi sağlar, böylece manuel dönüşüm gerekmez.

## Genel Yapısı

### 1. `Result<T>` Sınıfının Yapısı

`Result<T>` sınıfı, işlem sonucunu tutan bir yapıdır. Başarı veya hata durumuna göre uygun yanıtı döndüren `ExecuteResultAsync` metodunu içerir.

#### `Result<T>` Sınıfının Temel Yapısı

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

    // Başarı durumunu döndüren metod
    public static Result<T> Success(T value) => new(value);

    // Hata durumunu döndüren metod
    public static Result<T> Failure(Error error) => new(error);

    // IActionResult implementasyonu ile HTTP yanıtlarını döndüren metod
    public async Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Response.ContentType = "application/json";

        if (!IsError)
        {
            // Başarı durumu (200 OK)
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

## Örnek Kullanımı

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
