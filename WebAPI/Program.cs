using System.Text;
using System.Text.Json.Serialization;
using BusinessObject.Context;
using BusinessObject.DTO.Chat;
using BusinessObject.DTO.Noti;
using BusinessObject.DTO.Notification;
using Core.Middleware;
using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Service.Implement;
using WebAPI;
using WebAPI.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins("https://localhost:5243") // Chỉ cho phép yêu cầu từ localhost:5243
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
// Add services to the container.

builder.Services.AddInfra(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<CustomFlowerChainContext>();
//var configuration = builder.Configuration.Get<AppConfiguration>();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddHostedService<PromotionBackgroundService>();
builder.Services.AddHostedService<AutoUpdateOrderService>();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddHangfire(x => x.UseSqlServerStorage("DBDefault"));
// builder.Services.AddHangfireServer();
//builder.Services.AddSwaggerGen();
builder
    .Services.AddControllers()
    .AddJsonOptions(x => { x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo() { Title = "CustomFlowerChain API", Version = "v1" });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new List<string>()
            }
        }
    );
});
string? jwtIssuer = builder.Configuration[BusinessObject.DTO.Commons.JwtConstants.JwtIssuer];
string? jwtKey = builder.Configuration[BusinessObject.DTO.Commons.JwtConstants.JwtKey];
string? jwtAudience = builder.Configuration[BusinessObject.DTO.Commons.JwtConstants.JwtAudience];
//string? jwtIssuer = builder.Configuration["JwtSettings:JwtIssuer"];
//string? jwtKey = builder.Configuration["JwtSettings:JwtKey"];
//string? jwtAudience = builder.Configuration["JwtSettings:JwtAudience"];
builder
    .Services.AddAuthentication()
    .AddCookie()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
        };
    });
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000","http://localhost:5173")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();

//}
app.UseMiddleware<ExceptionHandlingMiddleware>();


//app.UseDefaultFiles();

//app.UseStaticFiles();

//app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();  // Kích hoạt HTTP Strict Transport Security (HSTS)
}
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapHub<ChatHub>("/chatHub");
app.UseAuthentication();
app.MapHub<NotificationHub>("/notificationHub");
app.UseAuthorization();
app.MapControllers();

app.Run();


