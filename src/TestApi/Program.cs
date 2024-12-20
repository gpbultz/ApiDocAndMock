
using ApiDocAndMock.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMockAuthentication();
builder.Services.AddMockSwagger(includeSecurity: true, includAnnotations: true);
builder.Services.AddMemoryDb();



var app = builder.Build();

app.UseMockAuthentication();
app.UseMockOutcome();
app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
