using BusinessObject.Context;
using BusinessObject.DTO.Chat;
using BusinessObject.DTO.Notification;
using Core.Middleware;
using Hangfire;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Service.Implement;
using WebAPI;

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
app.UseAuthorization();
app.MapControllers();

app.Run();


