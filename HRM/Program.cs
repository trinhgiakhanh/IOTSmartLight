using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Coravel;
using FluentValidation;
using HRM.Apis.Setting;
using HRM.Apis.Swagger;
using HRM.Data.Data;
using HRM.Data.DbContexts.Entities;
using HRM.Data.Entities;
using HRM.Data.Jwt;
using HRM.Repositories;
using HRM.Repositories.Base;
using HRM.Repositories.Dtos.Models;
using HRM.Repositories.Dtos.Results;
using HRM.Repositories.Helper;
using HRM.Repositories.Setting;
using HRM.Services.MQTT;
using HRM.Services.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.Configuration;
using System.Reflection;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add DB Connect SQLServer
builder.Services.AddDbContext<SmartlightDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SmartlightDbContext")));
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin",
        builder => builder
            .WithOrigins("http://127.0.0.1:5500", "http://localhost:3000") // Điền vào tên miền của dự án giao diện của bạn
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials() // Cho phép sử dụng credentials (cookies, xác thực)
    );
});
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
})
.AddMvc() // This is needed for controllers
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

#region + Validation
//Validation
builder.Services.AddScoped<IValidator<AccountUpdate>, AccountUpdateValidator>();
builder.Services.AddScoped<IValidator<AccountLogin>, AccountLoginValidator>();
#endregion
builder.Services.AddSingleton<HRM.Services.MQTT.MqttService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<HRM.Services.MQTT.MqttService>());

builder.Services.AddHostedService<HRM.Services.MQTT.MqttService>();

builder.Services.AddHostedService<MqttService>();
builder.Services.AddSingleton<MqttService>();


#region + Repositories
//Repositories
builder.Services.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));
//builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

#endregion



#region + Services

//Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDeviceControlService, DeviceControlService>();
#endregion



#region + Mapper


//Mapper
builder.Services.AddAutoMapper(typeof(MappingProfile));


#endregion

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddCookie(x =>
    {
        x.Cookie.Name = "token";
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            //2 dong
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["X-Access-Token"];
                return Task.CompletedTask;
            }
        };
    });


#region Authorization


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(HRM.Data.DbContexts.Entities.RoleExtensions.ADMIN_ROLE, policy => policy.RequireClaim("Role", HRM.Data.DbContexts.Entities.Role.Admin.ToString())); 
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(HRM.Data.DbContexts.Entities.RoleExtensions.PARTIME_ROLE, policy => policy.RequireClaim("Role", HRM.Data.DbContexts.Entities.Role.Partime.ToString())); 
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(HRM.Data.DbContexts.Entities.RoleExtensions.FULLTIME_ROLE, policy => policy.RequireClaim("Role", HRM.Data.DbContexts.Entities.Role.FullTime.ToString()));
});


#endregion



//Logging
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration)
);


#region  Setting


//Setting config
builder.Services.Configure<EmailSetting>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<CompanySetting>(builder.Configuration.GetSection("Company"));

#endregion



#region Scheduler
builder.Services.AddScheduler();

#endregion


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
    options.ExampleFilters();
});
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();
var app = builder.Build();


//Seeding data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Hiển thị từng phiên bản trong UI Swagger
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                                    description.GroupName.ToUpperInvariant());
        }
    });
}
app.UseSerilogRequestLogging();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseJwtMiddleware();
app.UseCors("AllowOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

