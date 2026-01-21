using ConstructureInfrastructure;
using ConstructorPresentation;
using ConstructorUseCases.ExportBlock;
using ConstructorUseCases.ExportTemplate;
using ConstructorUseCases.ImportBlock;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddConstructorInfrastructure();

builder.Services.AddScoped<IExportBlockRequestHandler, ExportBlockRequestHandler>();
builder.Services.AddScoped<IExportTemplateRequestHandler, ExportTemplateRequestHandler>();
builder.Services.AddScoped<IImportBlockRequestHandler, ImportBlockRequestHandler>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapConstructorEndpoints();

app.Run();