using GeoLocation_API.DB;
using GeoLocation_API.Repository.IRepositoryServices;
using GeoLocation_API.Repository.RepositoryServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5111);   // http
    options.ListenLocalhost(7038, listenOptions =>
    {

        listenOptions.UseHttps(options =>
        {
            options.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
        });
        listenOptions.UseConnectionLogging();
    });
});
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(option => {

    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(option =>
    {
        option.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };

    });

//builder.Services.AddSingleton<HttpClient>();
var userAgent = builder.Configuration["Nominatim:UserAgent"];
// Program.cs
builder.Services.AddHttpClient<IGeoLocationServices, GeoLocationServices>(client =>
{
    // ✅ Remove default User-Agent first
    client.DefaultRequestHeaders.Clear();

    // ✅ Set custom User-Agent
    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "GeoLocationApp/1.0 (itemad.hyder1997@gmail.com)");
    client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://myapp.com");
    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en");
});
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDapperDbConnection, DapperDbConnection>();
builder.Services.AddScoped<IGeoLocationServices, GeoLocationServices>();
//builder.Services.AddScoped<ICommonServices, CommonServices>();
//builder.Services.AddScoped<IBrokerSupportService, BrokerSupportService>();

//builder.Services.Configure<ExcelFileSettings>(builder.Configuration.GetSection("ExcelFileSettings"));
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSingleton(new SqlConnection(connectionString));

//builder.Services.Configure<ZohoOAuthSettings>(builder.Configuration.GetSection("ZohoOAuth"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add Bearer Authorization to Swagger UI
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter JWT with Bearer in the following format: Bearer {your token}",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    //options.OperationFilter<FileUploadOperation>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseCors(policy =>
{
    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
});
// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
