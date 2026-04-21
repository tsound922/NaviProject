using System.Text;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NaviProject.Api.Services;
using NaviProject.Core.Interfaces;
using NaviProject.Core.Services;
using NaviProject.Infrastructure.Repositories;
using NaviProject.Infrastructure.TypeHandlers;
using Npgsql;
using OllamaSharp;
using Pgvector.Dapper;

var builder = WebApplication.CreateBuilder(args);

// Dapper
SqlMapper.AddTypeHandler(new Pgvector.Dapper.VectorTypeHandler());
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// PostgreSQL DataSource
var connectionString = builder.Configuration.GetConnectionString("Postgres")!;
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.UseVector();
var dataSource = dataSourceBuilder.Build();
builder.Services.AddSingleton(dataSource);

// Ollama
var ollamaUrl = builder.Configuration["Ollama:BaseUrl"]!;
var httpClient = new HttpClient
{
    BaseAddress = new Uri(ollamaUrl),
    Timeout = TimeSpan.FromMinutes(10)
};
var ollamaClient = new OllamaApiClient(httpClient);
builder.Services.AddSingleton(ollamaClient);

// JWT
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// Repositories
builder.Services.AddScoped<IChatRepository>(sp => new ChatRepository(connectionString));
builder.Services.AddScoped<IChatMessageRepository>(sp => new ChatMessageRepository(connectionString));
builder.Services.AddScoped<IRagRepository>(sp => new RagRepository(dataSource));
builder.Services.AddScoped<IUserRepository>(sp => new UserRepository(connectionString));

// Services
builder.Services.AddScoped<IEmbeddingService, OllamaEmbeddingService>();
builder.Services.AddScoped<ILanguageModelService, OllamaLanguageModelService>();
builder.Services.AddScoped<IAuthService>(sp => new AuthService(jwtSecret));
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<RagService>();
builder.Services.AddScoped<ConversationService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IJiraService, JiraService>();
builder.Services.AddScoped<IConfluenceService, ConfluenceService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token here"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();