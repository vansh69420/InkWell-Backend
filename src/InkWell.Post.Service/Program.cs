using InkWell.Post.Service.DbContexts;
using InkWell.Post.Service.Repositories;
using InkWell.Post.Service.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy.WithOrigins(
                builder.Configuration["Frontend:Origin"] ?? "http://localhost:4200",
                "http://localhost:5000"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// PostgreSQL
builder.Services.AddDbContext<PostDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostDb")));

// ✅ Repository
builder.Services.AddScoped<IPostRepository, PostRepository>();

// ✅ Service layer (NEW)
builder.Services.AddScoped<IPostService, PostServiceImpl>();

// ✅ Needed for session access inside service
builder.Services.AddHttpContextAccessor();

// ✅ Session config (for unique view count)
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(24);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendCors");

app.UseHttpsRedirection();

// ✅ IMPORTANT: session must come before authorization
app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();