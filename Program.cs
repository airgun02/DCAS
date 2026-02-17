using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DCAS.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DCASContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DCASContext") ?? throw new InvalidOperationException("Connection string 'DCASContext' not found.")));

// Add authentication services to the container.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// run safe schema adjustments at startup so model/schema stay in sync during development
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupSchema");
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<DCASContext>();

        // Ensure Payments and PaymentMedicine tables exist (keeps parity with your HomeController checks).
        db.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Payments')
BEGIN
  CREATE TABLE Payments(
    PaymentId INT PRIMARY KEY IDENTITY,       
    Cash DECIMAL(18, 2),                      
    DoctorName VARCHAR(255),                  
    Status VARCHAR(100),                      
    PaymentMethod VARCHAR(100),                
    AmountPaid DECIMAL(10, 2),                
    AmountChanged DECIMAL(10, 2),             
    DentistFee DECIMAL(10, 2),                
    PaymentDate DATE,                     
    PersonInfoId INT,                         
    ServicesId INT
  );
END
");

        db.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PaymentMedicine')
BEGIN
CREATE TABLE PaymentMedicine (
    PaymentMedicineId INT PRIMARY KEY IDENTITY,
    PaymentId INT NOT NULL,
    MedicineId INT NULL,
    MedicineName NVARCHAR(MAX) NOT NULL,
    Price DECIMAL(10, 2) NULL,
    FOREIGN KEY(PaymentId) REFERENCES Payments(PaymentId)
);
END
");

        // Add missing columns if they do not exist (safe ALTERs)
        db.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'MedicineId' AND Object_ID = Object_ID('dbo.PaymentMedicine'))
BEGIN
    ALTER TABLE PaymentMedicine ADD MedicineId INT NULL;
END
");
        db.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'Quantity' AND Object_ID = Object_ID('dbo.PaymentMedicine'))
BEGIN
    ALTER TABLE PaymentMedicine ADD Quantity INT NOT NULL CONSTRAINT DF_PaymentMedicine_Quantity DEFAULT 1;
END
");
        db.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'UnitPrice' AND Object_ID = Object_ID('dbo.PaymentMedicine'))
BEGIN
    ALTER TABLE PaymentMedicine ADD UnitPrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_PaymentMedicine_UnitPrice DEFAULT 0;
END
");
        logger.LogInformation("Startup schema checks applied successfully.");
    }
    catch (Exception ex)
    {
        // log and continue; this will help identify schema issues during startup
        var logger2 = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupSchema");
        logger2.LogError(ex, "Error applying startup schema changes. If running in production use EF Migrations instead.");
    }

    // one-time data fix: mark existing PersonInfo rows with NULL status as Pre-register
    scope.ServiceProvider.GetRequiredService<DCASContext>()
         .Database.ExecuteSqlRaw(@"UPDATE PersonInfo
                               SET Status = 'Pre-register'
                               WHERE Status IS NULL;");

    scope.ServiceProvider.GetRequiredService<DCASContext>()
         .Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'WalkInStatus' AND Object_ID = Object_ID('dbo.PersonInfo'))
BEGIN
    ALTER TABLE PersonInfo ADD WalkInStatus NVARCHAR(50) NULL;
END
");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// IMPORTANT: The order matters. UseAuthentication must be before UseAuthorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
