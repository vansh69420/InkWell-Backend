using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddRouting();
builder.Services.AddControllers();

builder.Services.AddHttpClient("auth", client =>
{
    client.BaseAddress = new Uri("http://localhost:5077");
});

builder.Services.AddHttpClient("posts", client =>
{
    client.BaseAddress = new Uri("http://localhost:5103");
});

builder.Services.AddHttpClient("comments", client =>
{
    client.BaseAddress = new Uri("http://localhost:5175");
});

builder.Services.AddHttpClient("newsletter", client =>
{
    client.BaseAddress = new Uri("http://localhost:5011");
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("Set-Cookie");
    });
});

var app = builder.Build();

app.UseCors("FrontendCors");

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

await app.UseOcelot();

app.Run();