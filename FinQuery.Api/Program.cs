using FinQuery.Api.Data;
using FinQuery.Api.Services;
using FinQuery.Api.Services.Evaluation;
using FinQuery.Api.Services.Ingestion;
using FinQuery.Api.Services.Search;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register CORS for Next.js frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://127.0.0.1:3000", "http://127.0.0.1:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Register Vector Store & AI Services
builder.Services.AddSingleton<PostgresVectorStore>();
builder.Services.AddSingleton<Bm25Index>();
builder.Services.AddSingleton<FoundryLocalService>();
builder.Services.AddSingleton<EmbeddingService>();
builder.Services.AddSingleton<PromptService>();

builder.Services.AddScoped<RetrievalService>();
builder.Services.AddScoped<ChatCompletionService>();
builder.Services.AddScoped<CsvIngestionService>();
builder.Services.AddScoped<PdfVisionIngestionService>();
builder.Services.AddScoped<HitRateEvaluator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowNextJs");
app.UseAuthorization();
app.MapControllers();

app.Run();

