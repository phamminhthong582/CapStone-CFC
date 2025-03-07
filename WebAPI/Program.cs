using BusinessObject.Context;
using BusinessObject.DTO.Notification;
using Core.Middleware;
using Hangfire;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Service.Implement;
using WebAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddInfra(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<CustomFlowerChainContext>();
//var configuration = builder.Configuration.Get<AppConfiguration>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddHostedService<PromotionBackgroundService>();
builder.Services.AddHostedService<AutoUpdateOrderService>();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddHangfire(x => x.UseSqlServerStorage("DBDefault"));
// builder.Services.AddHangfireServer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(sw =>
{
    sw.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "1.0" });
    sw.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insert JWT Token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    sw.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
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

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");
app.UseCors("AllowAll");
// app.MapHub<NotificationHub>("/notificationHub");
app.UseAuthorization();

app.MapControllers();

app.Run();


