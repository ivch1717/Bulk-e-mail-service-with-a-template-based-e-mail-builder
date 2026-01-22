using UseCases;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddUseCases();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();


var app = builder.Build();
app.MapControllers(); 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
} // TODO: убрать сваггер после написания фронта
app.Run();