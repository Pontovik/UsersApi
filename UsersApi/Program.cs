using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UsersApi.Handlers;
using UsersApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication("BasicAuthentication")
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
builder.Services.AddAuthorization();
string connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddEntityFrameworkNpgsql().AddDbContext<AppDbContext>(options =>
		{
			options.UseNpgsql(connection);
		});
builder.Services.AddScoped<IUsersApiDbContext, AppDbContext>();
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
