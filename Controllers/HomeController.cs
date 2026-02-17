using DCAS.Data;
using DCAS.Models;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DCAS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DCASContext _context;

        public HomeController(ILogger<HomeController> logger, DCASContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void ExistingTable()
        {
            var tableCheckQueryOne = @"
-- Create PersonInfo (nullable columns) including WalkInStatus
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PersonInfo')
BEGIN
    CREATE TABLE dbo.PersonInfo (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(255) NULL,
        DetailOnee NVARCHAR(255) NULL,
        HomeAddress NVARCHAR(255) NULL,
        Date DATETIME NULL,
        BirthDay DATETIME NULL,
        Age INT NULL,
        HomeNumber NVARCHAR(50) NULL,
        Occupation NVARCHAR(100) NULL,
        MobileNumber NVARCHAR(50) NULL,
        OfficeAddress NVARCHAR(255) NULL,
        EmailAddress NVARCHAR(255) NULL,
        Status NVARCHAR(50) NULL,
        NameOfSpouse NVARCHAR(255) NULL,
        PersonalResponsibleforAccount NVARCHAR(255) NULL,
        Relationship NVARCHAR(100) NULL,
        DetailTwoo NVARCHAR(255) NULL,
        PhysicianCare NVARCHAR(255) NULL,
        PhysicianName NVARCHAR(255) NULL,
        ContactNumber NVARCHAR(50) NULL,
        MedicalServices NVARCHAR(250) NULL,
        Price DECIMAL(10, 2) NULL,
        DetailOne NVARCHAR(255) NULL,
        DetailTwo NVARCHAR(255) NULL,
        DetailThree NVARCHAR(255) NULL,
        DetailFour NVARCHAR(255) NULL,
        DetailFive NVARCHAR(255) NULL,
        DetailSix NVARCHAR(255) NULL,
        DetailSeven NVARCHAR(255) NULL,
        DetailEigth NVARCHAR(255) NULL,
        AvailableDay NVARCHAR(50) NULL,
        AvailableTime NVARCHAR(50) NULL,
        WalkInStatus NVARCHAR(50) NULL
    );
END;

-- Create TodaySchedule
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TodaySchedule')
BEGIN
    CREATE TABLE dbo.TodaySchedule (
        Id INT PRIMARY KEY IDENTITY(1,1),
        EventDate DATETIME NULL,
        EventTime TIME NULL,
        PersonName NVARCHAR(255) NULL
    );
END;

-- Create Services
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Services')
BEGIN
    CREATE TABLE dbo.Services (
        Id INT PRIMARY KEY IDENTITY(1,1),
        MedicalNameService NVARCHAR(255) NULL,
        Price DECIMAL(10,2) NULL
    );
END;

-- Create MedicineInventory
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MedicineInventory')
BEGIN
    CREATE TABLE dbo.MedicineInventory (
        Id INT PRIMARY KEY IDENTITY(1,1),
        MedicineName NVARCHAR(255) NULL,
        Price DECIMAL(10,2) NULL,
        Quantity INT NULL,
        Miligram NVARCHAR(MAX) NULL,
        Description NVARCHAR(MAX) NULL,
        ExpiryDate DATE NULL,
        Image VARBINARY(MAX) NULL
    );
END;

-- Create Users
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
BEGIN
    CREATE TABLE dbo.Users (
        UsersId INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(255) NULL,
        Address NVARCHAR(255) NULL,
        Email NVARCHAR(255) NULL,
        PhoneNumber NVARCHAR(20) NULL,
        Role NVARCHAR(50) NULL,
        JobTitle NVARCHAR(50) NULL,
        Specialization NVARCHAR(50) NULL,
        Gender NVARCHAR(10) NULL,
        Nationality NVARCHAR(50) NULL,
        Position NVARCHAR(50) NULL,
        WorkStatus NVARCHAR(20) NULL,
        Age NVARCHAR(5) NULL,
        BirthDate DATETIME NULL,
        StartDate DATETIME NULL,
        Profile VARBINARY(MAX) NULL,
        Username NVARCHAR(50) NULL,
        Password NVARCHAR(255) NULL
    );
END;

-- Create Payments
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Payments')
BEGIN
    CREATE TABLE dbo.Payments (
        PaymentId INT PRIMARY KEY IDENTITY(1,1),
        Cash DECIMAL(18,2) NULL,
        DoctorName NVARCHAR(255) NULL,
        Status NVARCHAR(100) NULL,
        PaymentMethod NVARCHAR(100) NULL,
        AmountPaid DECIMAL(10,2) NULL,
        AmountChanged DECIMAL(10,2) NULL,
        DentistFee DECIMAL(10,2) NULL,
        PaymentDate DATE NULL,
        PersonInfoId INT NULL,
        ServicesId INT NULL,
        CONSTRAINT FK_Payments_PersonInfo FOREIGN KEY (PersonInfoId) REFERENCES dbo.PersonInfo(Id),
        CONSTRAINT FK_Payments_Services FOREIGN KEY (ServicesId) REFERENCES dbo.Services(Id)
    );
END;

-- Create PaymentMedicine
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PaymentMedicine')
BEGIN
    CREATE TABLE dbo.PaymentMedicine (
        PaymentMedicineId INT PRIMARY KEY IDENTITY(1,1),
        PaymentId INT NULL,
        MedicineId INT NULL,
        MedicineName NVARCHAR(MAX) NULL,
        UnitPrice DECIMAL(18,2) NULL,
        Quantity INT NULL,
        Price DECIMAL(10,2) NULL,
        CONSTRAINT FK_PaymentMedicine_Payments FOREIGN KEY (PaymentId) REFERENCES dbo.Payments(PaymentId)
    );
END;

-- Create PaymentServices
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PaymentServices')
BEGIN
    CREATE TABLE dbo.PaymentServices (
        Id INT PRIMARY KEY IDENTITY(1,1),
        PaymentId INT NULL,
        ServiceId INT NULL,
        UnitPrice DECIMAL(18,2) NULL,
        CONSTRAINT FK_PaymentServices_Payments FOREIGN KEY (PaymentId) REFERENCES dbo.Payments(PaymentId),
        CONSTRAINT FK_PaymentServices_Services FOREIGN KEY (ServiceId) REFERENCES dbo.Services(Id)
    );
END;

-- Add missing columns if they don't exist (safe ALTERs)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'MedicineId' AND Object_ID = Object_ID('dbo.PaymentMedicine'))
BEGIN
    ALTER TABLE dbo.PaymentMedicine ADD MedicineId INT NULL;
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'Quantity' AND Object_ID = Object_ID('dbo.PaymentMedicine'))
BEGIN
    ALTER TABLE dbo.PaymentMedicine ADD Quantity INT NULL;
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'UnitPrice' AND Object_ID = Object_ID('dbo.PaymentMedicine'))
BEGIN
    ALTER TABLE dbo.PaymentMedicine ADD UnitPrice DECIMAL(18,2) NULL;
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'WalkInStatus' AND Object_ID = Object_ID('dbo.PersonInfo'))
BEGIN
    ALTER TABLE dbo.PersonInfo ADD WalkInStatus NVARCHAR(50) NULL;
END;

-- Normalize rows: move walk-in values from Status -> WalkInStatus when appropriate
-- (this moves Pre-register/Registered stored in Status into WalkInStatus and clears Status)
UPDATE dbo.PersonInfo
SET
    WalkInStatus = CASE
        WHEN (LTRIM(RTRIM(Status)) IN ('Pre-register','Registered')) AND (WalkInStatus IS NULL OR LTRIM(RTRIM(WalkInStatus)) = '')
            THEN LTRIM(RTRIM(Status))
        ELSE WalkInStatus
    END,
    Status = CASE
        WHEN LTRIM(RTRIM(Status)) IN ('Pre-register','Registered') THEN NULL
        ELSE Status
    END
WHERE LTRIM(RTRIM(Status)) IN ('Pre-register','Registered');

-- Also mark WalkInStatus = 'Pre-register' for records that have payments or today schedule but no walkin flag (optional)
UPDATE dbo.PersonInfo
SET WalkInStatus = 'Pre-register'
WHERE (WalkInStatus IS NULL OR LTRIM(RTRIM(WalkInStatus)) = '')
  AND (
        Id IN (SELECT DISTINCT PersonInfoId FROM dbo.Payments WHERE PersonInfoId IS NOT NULL)
        OR EXISTS (SELECT 1 FROM dbo.TodaySchedule ts WHERE ts.PersonName = dbo.PersonInfo.Name)
      );
";
            _context.Database.ExecuteSqlRaw(tableCheckQueryOne);

            var tableCheckQueryTwo = @"
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TodaySchedule')
    BEGIN
        CREATE TABLE TodaySchedule (
            Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
            EventDate DATETIME NOT NULL,
            EventTime TIME NOT NULL,
            PersonName NVARCHAR(255) NOT NULL
        );
    END";

            _context.Database.ExecuteSqlRaw(tableCheckQueryTwo);

            var tableCheckQueryServices = @"
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Services')
    BEGIN
        CREATE TABLE Services (
            Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
            MedicalNameService NVARCHAR(255) NOT NULL,
            Price DECIMAL(10,2) NOT NULL
        );
    END";
            _context.Database.ExecuteSqlRaw(tableCheckQueryServices);

            var tableCheckQueryMedicineInventory = @"
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MedicineInventory')
    BEGIN
         CREATE TABLE MedicineInventory (
    Id INT PRIMARY KEY  identity(1,1),
    MedicineName VARCHAR(255) NOT NULL,
    Price Decimal(10,2) not null,
    Quantity INT NOT NULL,
    Miligram Nvarchar(Max),
    Description Nvarchar(Max),
    ExpiryDate DATE NOT NULL,
Image VARBINARY(MAX) 
        );
    END";
            _context.Database.ExecuteSqlRaw(tableCheckQueryMedicineInventory);

            var tableCheckQueryUsers = @"
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
    BEGIN
    CREATE TABLE Users (
    UsersId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255),
    Address NVARCHAR(255),
    Email NVARCHAR(255),
    PhoneNumber NVARCHAR(20),
    Role NVARCHAR(50),
    JobTitle NVARCHAR(50),
    Specialization NVARCHAR(50),
    Gender NVARCHAR(10),
    Nationality NVARCHAR(50),
    Position NVARCHAR(50),
    WorkStatus NVARCHAR(20),
    Age NVARCHAR(5),
    BirthDate DATETIME,
    StartDate DATETIME,
    Profile VARBINARY(MAX),
    Username NVARCHAR(50),
    Password NVARCHAR(255)
        );
    END";
            _context.Database.ExecuteSqlRaw(tableCheckQueryUsers);

            var tableCheckQueryPayment = @"
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
    ServicesId INT,                           
                           


    FOREIGN KEY(PersonInfoId) REFERENCES PersonInfo(Id),  
    FOREIGN KEY(ServicesId) REFERENCES Services(Id)      );
    END";
            _context.Database.ExecuteSqlRaw(tableCheckQueryPayment);

            var tableCheckQueryPaymentMedicine = @"
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PaymentMedicine')
    BEGIN
    CREATE TABLE PaymentMedicine (
    PaymentMedicineId INT PRIMARY KEY IDENTITY,
    PaymentId INT NOT NULL,
    MedicineId INT NULL,
    MedicineName       NVARCHAR(max) not null,
    Price DECIMAL(10, 2) NULL,
    
    FOREIGN KEY(PaymentId) REFERENCES Payments(PaymentId)
);
    END";
            _context.Database.ExecuteSqlRaw(tableCheckQueryPaymentMedicine);

            // If columns are missing (model updated), add them safely.
            var addMedicineIdColumn = @"
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'MedicineId' AND Object_ID = Object_ID('dbo.PaymentMedicine'))
    BEGIN
        ALTER TABLE PaymentMedicine ADD MedicineId INT NULL;
    END";
            _context.Database.ExecuteSqlRaw(addMedicineIdColumn);

            var addQuantityColumn = @"
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'Quantity' AND Object_ID = Object_ID('dbo.PaymentMedicine'))
    BEGIN
        ALTER TABLE PaymentMedicine ADD Quantity INT NULL;
    END";
            _context.Database.ExecuteSqlRaw(addQuantityColumn);

            var addUnitPriceColumn = @"
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'UnitPrice' AND Object_ID = Object_ID('dbo.PaymentMedicine'))
    BEGIN
        ALTER TABLE PaymentMedicine ADD UnitPrice DECIMAL(18,2) NULL;
    END";
            _context.Database.ExecuteSqlRaw(addUnitPriceColumn);

            var tableCheckPaymentServices = @"
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PaymentServices')
    BEGIN
        CREATE TABLE [dbo].[PaymentServices](
            [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            [PaymentId] INT NOT NULL,
            [ServiceId] INT NOT NULL,
            [UnitPrice] DECIMAL(18,2) NOT NULL,
            CONSTRAINT FK_PaymentServices_Payments FOREIGN KEY (PaymentId) REFERENCES Payments(PaymentId),
            CONSTRAINT FK_PaymentServices_Services FOREIGN KEY (ServiceId) REFERENCES Services(Id)
        );
    END";
            _context.Database.ExecuteSqlRaw(tableCheckPaymentServices);

            var addWalkInStatusColumn = @"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'WalkInStatus' AND Object_ID = Object_ID('dbo.PersonInfo'))
BEGIN
    ALTER TABLE PersonInfo ADD WalkInStatus NVARCHAR(50) NULL;
END";
            _context.Database.ExecuteSqlRaw(addWalkInStatusColumn);

            // Normalize existing rows: set WalkInStatus = 'Registered' for rows that
            // are NULL or empty AND have either a payment or a TodaySchedule entry.
            // This keeps behaviour targeted (won't overwrite intentional statuses).
            _context.Database.ExecuteSqlRaw(@"
UPDATE PersonInfo
SET WalkInStatus = 'Registered'
WHERE (WalkInStatus IS NULL OR LTRIM(RTRIM(WalkInStatus)) = '')
  AND (
        Id IN (SELECT DISTINCT PersonInfoId FROM Payments WHERE PersonInfoId IS NOT NULL)
        OR EXISTS (SELECT 1 FROM TodaySchedule ts WHERE ts.PersonName = PersonInfo.Name)
      );
");
            var tableCheckLatestUpdates = @"
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LatestUpdates')
BEGIN
    CREATE TABLE dbo.LatestUpdates (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Title NVARCHAR(200) NOT NULL,
        Summary NVARCHAR(500) NULL,
        Body NVARCHAR(MAX) NULL,
        Image VARBINARY(MAX) NULL,
        ImageContentType NVARCHAR(100) NULL,
        CreatedAt DATETIME NOT NULL
    );
END";
            _context.Database.ExecuteSqlRaw(tableCheckLatestUpdates);
        }



        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Index()
        {
            ExistingTable();

            try
            {
                var latest = await _context.LatestUpdates
                    .OrderByDescending(u => u.CreatedAt)
                    .FirstOrDefaultAsync();
                ViewBag.LatestUpdate = latest;
            }
            catch (Exception)
            {
                // If LatestUpdates table (or other DB error) is missing, show no latest update
                ViewBag.LatestUpdate = null;
            }

            return View();
        }

        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Profile()
        {
            // Primary: try to resolve user by UsersId claim (set at login)
            var userIdClaim = User.FindFirst("UsersId")?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var uid))
            {
                var byId = await _context.Users.FindAsync(uid);
                if (byId != null)
                    return View("Profile", byId);
            }

            // Fallback: use Name claim (may be either username or display name depending on login implementation)
            var nameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(nameClaim))
            {
                return NotFound(); // No usable claim found
            }

            // Try both Username and Name fields when matching the claim value
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == nameClaim || u.Name == nameClaim);

            if (currentUser == null)
            {
                return NotFound();
            }

            return View("Profile", currentUser);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
