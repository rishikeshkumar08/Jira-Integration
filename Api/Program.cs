using Api.Masters;
using Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUi", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<JiraPocMaster>();

builder.Services.Configure<Api.Models.JiraSettings>(
    builder.Configuration.GetSection("Jira"));

builder.Services.AddHttpClient<Api.Services.JiraRestClient>();

var app = builder.Build();

app.UseCors("AllowUi");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseMiddleware<JiraWebhookAuthMiddleware>();
app.MapControllers();
app.Run();