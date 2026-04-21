using InkWell.Taxonomy.Service.DbContexts;
using InkWell.Taxonomy.Service.Repositories;
using InkWell.Taxonomy.Service.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (same policy name as your other services)
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

// DB
builder.Services.AddDbContext<TaxonomyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TaxonomyDb")));

// Repos
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();

// Services
builder.Services.AddScoped<ICategoryService, CategoryServiceImpl>();
builder.Services.AddScoped<ITagService, TagServiceImpl>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendCors");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();