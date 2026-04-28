namespace NTR.API.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string API_KEY_HEADER = "X-Api-Key";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            // Swagger'a izin ver
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            // Header'da API key var mı kontrol et
            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "API Key bulunamadı. Header'a X-Api-Key ekleyin."
                });
                return;
            }

            // API key doğru mu kontrol et
            string? apiKey = configuration.GetValue<string>("ApiKey");
            if (!apiKey!.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Geçersiz API Key."
                });
                return;
            }

            await _next(context);
        }
    }
}