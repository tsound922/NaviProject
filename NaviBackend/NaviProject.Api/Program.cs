using Dapper;
using NaviProject.Api.Services;
using NaviProject.Core.Interfaces;
using NaviProject.Core.Services;
using NaviProject.Infrastructure.Repositories;
using NaviProject.Infrastructure.TypeHandlers;
using Npgsql;
using OllamaSharp;
using Pgvector.Dapper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
SqlMapper.AddTypeHandler(new Pgvector.Dapper.VectorTypeHandler());

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

// Repositories
builder.Services.AddScoped<IChatRepository>(sp =>
    new ChatRepository(connectionString));
builder.Services.AddScoped<IChatMessageRepository>(sp =>
    new ChatMessageRepository(connectionString));
builder.Services.AddScoped<IRagRepository>(sp =>
    new RagRepository(connectionString));

// Services
builder.Services.AddScoped<IEmbeddingService, OllamaEmbeddingService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<RagService>();
builder.Services.AddScoped<ILanguageModelService, OllamaLanguageModelService>();
builder.Services.AddScoped<ConversationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();