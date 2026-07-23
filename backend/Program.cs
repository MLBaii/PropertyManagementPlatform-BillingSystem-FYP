using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PropertyBill.Api.Data;
using PropertyBill.Api.Repositories;
using PropertyBill.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options => options.AddPolicy("AdminPortal", policy => policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!)),
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IResidentRepository, ResidentRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
builder.Services.AddScoped<IBillRepository, BillRepository>();
builder.Services.AddScoped<IBillService, BillService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPaymentProofRepository, PaymentProofRepository>();
builder.Services.AddScoped<IPaymentProofService, PaymentProofService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationTokenRepository, NotificationTokenRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationTokenService, NotificationTokenService>();
builder.Services.AddScoped<INotificationSendingService, NotificationSendingService>();
builder.Services.AddScoped<IDisputeRepository, DisputeRepository>();
builder.Services.AddScoped<IDisputeService, DisputeService>();
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IPropertyService, PropertyService>();

builder.Services.AddHttpClient<ISupabaseStorageService, SupabaseStorageService>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var serviceRoleKey = config["Supabase:ServiceRoleKey"];
    client.DefaultRequestHeaders.Add("apikey", serviceRoleKey);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", serviceRoleKey);
});

builder.Services.AddHttpClient<IExpoPushService, ExpoPushService>(client =>
{
    client.BaseAddress = new Uri("https://exp.host/");
});

var app = builder.Build();

if (args.Contains("--seed") || args.Contains("--reseed"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (args.Contains("--reseed"))
    {
        await DbSeeder.ClearAsync(context);
    }
    await DbSeeder.SeedAsync(context);
    return;
}

if (args.Contains("--seed-extra"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedExtraAsync(context);
    return;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AdminPortal");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
