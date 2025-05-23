﻿using Microsoft.EntityFrameworkCore;
using ToHeBE.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ToHeBE.Models.Auth;

var builder = WebApplication.CreateBuilder(args);

string strcnn = builder.Configuration.GetConnectionString("cnn");
builder.Services.AddDbContext<ToHeDbContext>(options => options.UseSqlServer(strcnn));

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddAuthorization();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cấu hình JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = builder.Configuration["Jwt:Issuer"],  // "https://localhost:7111"
			ValidAudience = builder.Configuration["Jwt:Audience"],  // "https://localhost:7111"
			IssuerSigningKey = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
		};
	});

builder.Services.AddSingleton<EmailService>(); /*cấu hình gửi mail*/



var app = builder.Build();
app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
// Thêm middleware để phục vụ tệp tĩnh
app.UseStaticFiles();
/**/


app.UseRouting();

app.UseHttpsRedirection();


app.UseAuthentication(); // THIẾU DÒNG NÀY()
app.UseAuthorization();


app.MapControllers();

app.Run();
