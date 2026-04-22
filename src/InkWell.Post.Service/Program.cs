using System.Text;
using InkWell.Post.Service.DbContexts;
using InkWell.Post.Service.Repositories;
using InkWell.Post.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using InkWell.Post.Service.External;
using System.Security.Claims;

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

// Repositories + Services
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostServiceImpl>();

var taxonomyBaseUrl = builder.Configuration["Services:TaxonomyBaseUrl"];
if (string.IsNullOrWhiteSpace(taxonomyBaseUrl))
{
    throw new InvalidOperationException("Services:TaxonomyBaseUrl missing in config.");
}

builder.Services.AddHttpClient<ITaxonomyClient, TaxonomyClient>(client =>
{
    client.BaseAddress = new Uri(taxonomyBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

// HttpContext + Session (needed for unique view count per session)
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(24);
});

// ✅ JWT Auth (read token from cookie: inkwell_access_token)
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException("JWT configuration missing. Please set Jwt:Key, Jwt:Issuer, Jwt:Audience in appsettings.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };

        // Read token from cookie instead of Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("inkwell_access_token", out var token) &&
                    !string.IsNullOrWhiteSpace(token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendCors");


// Session must be before auth/authorization
app.UseSession();

// ✅ Auth middleware order
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();