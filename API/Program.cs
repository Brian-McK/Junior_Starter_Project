using API.Interfaces;
using API.Models;
using API.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<EmployeeSkillsDatabaseSettings>(
    builder.Configuration.GetSection("EmployeeSkillsDatabase"));

var hi = builder.Services.Configure<EmployeeSkillsDatabaseSettings>(
    builder.Configuration.GetSection("EmployeeSkillsDatabase"));


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ISkillLevelRepository, SkillLevelRepository>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();