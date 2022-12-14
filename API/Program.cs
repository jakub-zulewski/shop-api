using API.Data;
using API.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("Shop API", new OpenApiInfo
	{
		Title = "Shop API",
		Version = "v1"
	});
});
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddDbContext<StoreContext>(options =>
{
	options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddCors();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();

app.UseSwaggerUI(options =>
{
	options.SwaggerEndpoint("/swagger/Shop%20API/swagger.json", "Shop API v1");
	options.RoutePrefix = "api/docs";
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(options =>
{
	options.AllowAnyHeader()
		.AllowAnyMethod()
		.AllowCredentials()
		.WithOrigins("http://localhost:3000");
});

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateAsyncScope();
try
{
	var storeContext = scope.ServiceProvider.GetRequiredService<StoreContext>();

	await storeContext.Database.MigrateAsync();
	await DbInitializer.InitializeDatabase(storeContext);
}
catch (Exception ex)
{
	var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

	logger.LogError(ex, "Problem migrationg data.");
}

await app.RunAsync();
