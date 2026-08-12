using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Application.Mappings;
using SchoolCRM.Application.Validators.Auth;
using SchoolCRM.Infrastructure.Data;
using SchoolCRM.Domain.Entities.School;
using SchoolCRM.Infrastructure.Repositories;
using SchoolCRM.Infrastructure.Services;
using SchoolCRM.Domain.Entities.Identity;
using SchoolCRM.Domain.Entities.Library;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Domain.Enums;
using SchoolCRM.API.Middleware;
using SchoolCRM.Infrastructure.SignalR;
using Serilog;
using FluentValidation;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/school-crm-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")),
        b =>
        {
            b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            b.EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null);
        })
    .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
    .EnableDetailedErrors(builder.Environment.IsDevelopment()));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var claims = context.Principal?.Claims;
            var userId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    context.Fail("User not found or inactive");
                }
            }
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.SuperAdmin, Roles.SchoolAdmin));
    options.AddPolicy("Management", policy => policy.RequireRole(Roles.SuperAdmin, Roles.SchoolAdmin, Roles.Principal, Roles.VicePrincipal));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole(Roles.Teacher, Roles.ClassTeacher, Roles.Accountant, Roles.Receptionist, Roles.Librarian));
    options.AddPolicy("TeacherOnly", policy => policy.RequireRole(Roles.Teacher, Roles.ClassTeacher));

    foreach (var permission in Permissions.AllPermissions)
    {
        options.AddPolicy(permission, policy =>
            policy.RequireAssertion(context =>
                context.User.HasClaim("Permission", permission) ||
                context.User.IsInRole(Roles.SuperAdmin)));
    }
});

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:3000", "http://localhost:5173" };
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
var useRedis = !string.IsNullOrEmpty(redisConnectionString);

if (useRedis)
{
    try
    {
        var mux = StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnectionString + ",abortConnect=false,connectTimeout=3000,syncTimeout=3000");
        if (mux.IsConnected)
        {
            builder.Services.AddSignalR().AddStackExchangeRedis(redisConnectionString!);
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
            });
            Log.Information("Redis connected successfully for SignalR and caching");
        }
        else
        {
            throw new Exception("Redis not connected");
        }
    }
    catch (Exception ex)
    {
        Log.Warning("Redis unavailable, falling back to in-memory cache and basic SignalR: {Message}", ex.Message);
        builder.Services.AddSignalR();
        builder.Services.AddDistributedMemoryCache();
    }
}
else
{
    Log.Information("Redis not configured, using in-memory cache and basic SignalR");
    builder.Services.AddSignalR();
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddValidatorsFromAssembly(typeof(LoginValidator).Assembly);

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ISchoolRepository, SchoolRepository>();
builder.Services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
builder.Services.AddScoped<IClassRoomRepository, ClassRoomRepository>();
builder.Services.AddScoped<ISectionRepository, SectionRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ITimetableRepository, TimetableRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentDocumentRepository, StudentDocumentRepository>();
builder.Services.AddScoped<IStudentHealthRecordRepository, StudentHealthRecordRepository>();
builder.Services.AddScoped<IStudentLeaveRepository, StudentLeaveRepository>();
builder.Services.AddScoped<IParentRepository, ParentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<ITeacherLeaveRepository, TeacherLeaveRepository>();
builder.Services.AddScoped<ITeacherSalaryRepository, TeacherSalaryRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeLeaveRepository, EmployeeLeaveRepository>();
builder.Services.AddScoped<IEmployeeSalaryRepository, EmployeeSalaryRepository>();
builder.Services.AddScoped<IDesignationRepository, DesignationRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IExamScheduleRepository, ExamScheduleRepository>();
builder.Services.AddScoped<IMarkRepository, MarkRepository>();
builder.Services.AddScoped<IReportCardRepository, ReportCardRepository>();
builder.Services.AddScoped<IExamQuestionRepository, ExamQuestionRepository>();
builder.Services.AddScoped<IExamSubmissionRepository, ExamSubmissionRepository>();
builder.Services.AddScoped<IExamAnswerRepository, ExamAnswerRepository>();
builder.Services.AddScoped<IFeeStructureRepository, FeeStructureRepository>();
builder.Services.AddScoped<IFeeInstallmentRepository, FeeInstallmentRepository>();
builder.Services.AddScoped<IFeeReceiptRepository, FeeReceiptRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookIssueRepository, BookIssueRepository>();
builder.Services.AddScoped<ITransportRouteRepository, TransportRouteRepository>();
builder.Services.AddScoped<IHostelRoomRepository, HostelRoomRepository>();
builder.Services.AddScoped<IHostelRepository, HostelRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IAccountHeadRepository, AccountHeadRepository>();
builder.Services.AddScoped<IIncomeRepository, IncomeRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IParentService, ParentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IFeeService, FeeService>();
builder.Services.AddScoped<ISchoolService, SchoolService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILibraryService, LibraryService>();
builder.Services.AddScoped<ITransportService, TransportService>();
builder.Services.AddScoped<IHostelService, HostelService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IHomeworkService, HomeworkService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<INoticeService, NoticeService>();

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors.Select(x => x.ErrorMessage).ToList()
                );

            var allErrors = errors.SelectMany(e => e.Value).ToList();

            var response = ApiResponse.FailResponse(
                "Validation failed",
                StatusCodes.Status400BadRequest,
                allErrors
            );

            return new BadRequestObjectResult(response)
            {
                ContentTypes = { "application/json" }
            };
        };
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new SchoolCRM.API.Converters.TimeOnlyJsonConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "School CRM Management System API",
        Version = "v1",
        Description = "Production-grade School CRM Management System REST API",
        Contact = new OpenApiContact
        {
            Name = "School CRM",
            Email = "admin@schoolcrm.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });

    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "School CRM API v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    Log.Information("Database migrated successfully");

    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var role in Roles.AllRoles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = role, NormalizedName = role.ToUpper() });
        }
    }
    Log.Information("Roles seeded successfully");

    async Task SeedUser(string email, string firstName, string lastName, string gender, string role, string password)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null) return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Gender = Enum.Parse<Gender>(gender),
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
            Log.Information("Seeded user {Email} with role {Role}", email, role);
        }
        else
        {
            Log.Warning("Failed to seed user {Email}: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    await SeedUser("admin@schoolcrm.com", "Super", "Admin", "Male", Roles.SuperAdmin, "admin@123");
    await SeedUser("teacher@schoolcrm.com", "John", "Smith", "Male", Roles.Teacher, "Teacher@1234");
    await SeedUser("student@schoolcrm.com", "Jane", "Doe", "Female", Roles.Student, "Student@1234");
    await SeedUser("parent@schoolcrm.com", "Robert", "Doe", "Male", Roles.Parent, "Parent@1234");

    var school = db.Schools.FirstOrDefault();
    if (school is null)
    {
        school = new School
        {
            Id = Guid.NewGuid(),
            Name = "Springfield Academy",
            Code = "SA001",
            Email = "admin@springfield.edu",
            Phone = "1234567890",
            Address = "123 School Street",
            City = "Springfield",
            State = "Illinois",
            Country = "USA",
            PostalCode = "62701",
            CreatedAt = DateTime.UtcNow
        };
        db.Schools.Add(school);
        await db.SaveChangesAsync();

        var academicYear = new AcademicYear
        {
            Id = Guid.NewGuid(),
            Name = "2026-2027",
            StartDate = new DateTime(2026, 4, 1),
            EndDate = new DateTime(2027, 3, 31),
            IsCurrent = true,
            SchoolId = school.Id,
            CreatedAt = DateTime.UtcNow
        };
        db.AcademicYears.Add(academicYear);
        await db.SaveChangesAsync();

        school.CurrentAcademicYearId = academicYear.Id;
        db.Schools.Update(school);
        await db.SaveChangesAsync();

        for (int i = 1; i <= 12; i++)
        {
            var classId = Guid.NewGuid();
            var classRoom = new ClassRoom
            {
                Id = classId,
                Name = $"Class {i}",
                Code = $"CLS{i:D2}",
                Capacity = 40,
                SchoolId = school.Id,
                AcademicYearId = academicYear.Id,
                CreatedAt = DateTime.UtcNow
            };
            db.ClassRooms.Add(classRoom);

            foreach (var sectionName in new[] { "A", "B", "C" })
            {
                db.Sections.Add(new Section
                {
                    Id = Guid.NewGuid(),
                    Name = sectionName,
                    Code = $"{i}{sectionName}",
                    Capacity = 40,
                    ClassRoomId = classId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();
        Log.Information("Seeded school '{SchoolName}' with 12 classes and 36 sections", school.Name);
    }

    if (!db.Departments.Any())
    {
        var departmentNames = new[]
        {
            "Administration", "Mathematics", "Science", "English", "History",
            "Geography", "Computer Science", "Arts", "Physical Education",
            "Finance", "HR", "IT Support"
        };

        var departments = departmentNames.Select((name, index) => new Department
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = $"DEPT{(index + 1):D2}",
            SchoolId = school.Id,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        db.Departments.AddRange(departments);
        await db.SaveChangesAsync();
        Log.Information("Seeded {DepartmentCount} departments", departments.Count);
    }

    if (!db.Subjects.Any())
    {
        var subjectNames = new[] { "Mathematics", "Science", "English", "Social Studies", "Computer Science" };
        var classrooms = db.ClassRooms.Where(c => c.SchoolId == school.Id).ToList();

        var subjects = classrooms.SelectMany(classroom =>
            subjectNames.Select((name, index) => new Subject
            {
                Id = Guid.NewGuid(),
                Name = name,
                Code = $"SUB{name.Substring(0, 1).ToUpper()}{index + 1}",
                ClassRoomId = classroom.Id,
                TotalMarks = 100,
                PassMarks = 40,
                IsElective = false,
                SortOrder = index + 1,
                CreatedAt = DateTime.UtcNow
            })).ToList();

        db.Subjects.AddRange(subjects);
        await db.SaveChangesAsync();
        Log.Information("Seeded {SubjectCount} subjects across {ClassCount} classes", subjects.Count, classrooms.Count);
    }

    if (!db.BookCategories.Any())
    {
        var categoryNames = new[]
        {
            "Fiction", "Non-Fiction", "Science", "Mathematics", "History",
            "Literature", "Reference", "Technology", "Arts", "Other"
        };

        var categories = categoryNames.Select((name, index) => new BookCategory
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = $"CAT{(index + 1):D3}",
            SchoolId = school.Id,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        db.BookCategories.AddRange(categories);
        await db.SaveChangesAsync();
        Log.Information("Seeded {CategoryCount} book categories", categories.Count);
    }
}

app.Run();

public partial class Program { }
