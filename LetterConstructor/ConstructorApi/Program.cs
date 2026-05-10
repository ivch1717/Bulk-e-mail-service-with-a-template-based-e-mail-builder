using System.Text.Json.Serialization;
using ConstructureInfrastructure;
using ConstructorPresentation;
using ConstructorUseCases.AddFile;
using ConstructorUseCases.DeleteFileById;
using ConstructorUseCases.ExportBlock;
using ConstructorUseCases.ExportTemplate;
using ConstructorUseCases.GetAllFiles;
using ConstructorUseCases.GetFileById;
using ConstructorUseCases.ImportBlock;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddConstructorInfrastructure(builder.Configuration);

builder.Services.AddScoped<IExportBlockRequestHandler, ExportBlockRequestHandler>();
builder.Services.AddScoped<IExportTemplateRequestHandler, ExportTemplateRequestHandler>();
builder.Services.AddScoped<IImportBlockRequestHandler, ImportBlockRequestHandler>();

builder.Services.AddScoped<IAddFileRequestHandler, AddFileRequestHandler>();
builder.Services.AddScoped<IDeleteFileByIdRequestHandler, DeleteFileByIdRequestHandler>();
builder.Services.AddScoped<IGetFileByIdRequestHandler, GetFileByIdRequestHandler>();
builder.Services.AddScoped<IGetAllFilesRequestHandler, GetAllFilesRequestHandler>();

builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p => p
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders("Content-Disposition"));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapConstructorEndpoints();

app.Run();
