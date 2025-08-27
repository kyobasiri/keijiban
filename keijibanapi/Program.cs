// ===============================
// Program.cs - DI設定更新版（MariaDBキャッシュ対応）
// ===============================
using Dapper; // ★ using を追加
using FluentValidation;
using keijibanapi.Data.TypeHandlers; // ★ using を追加
using keijibanapi.Hubs;
using keijibanapi.Infrastructure;
using keijibanapi.Middleware;
using keijibanapi.Models;
using keijibanapi.Repositories;
using keijibanapi.Services;
using keijibanapi.Validators;


// Dapperでsnake_caseとPascalCaseを自動マッピングする設定
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

SqlMapper.AddTypeHandler(new StringTypeHandler<EmergencyPriority>());
SqlMapper.AddTypeHandler(new StringTypeHandler<Priority>());
SqlMapper.AddTypeHandler(new StringTypeHandler<ActionStatus>());

var builder = WebApplication.CreateBuilder(args);

Dapper.SqlMapper.AddTypeHandler(new EmergencyPriorityTypeHandler());

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddValidatorsFromAssemblyContaining<SendMessageRequestValidator>();

// CORS設定
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

// SignalR設定
builder.Services.AddSignalR();

// サービスの登録
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IDoctorAbsenceService, DoctorAbsenceService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmergencyNoticeService, EmergencyNoticeService>();
builder.Services.AddScoped<IExtraScheduleService, ExtraScheduleService>(); // ★ 新規追加


// --- リポジトリ層の登録 ---
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<IDoctorAbsenceRepository, DoctorAbsenceRepository>();
builder.Services.AddScoped<IExtraScheduleRepository, ExtraScheduleRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IEmergencyNoticeRepository, EmergencyNoticeRepository>();


// ログ設定
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS有効化
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// SignalRハブのマッピング
app.MapHub<KeijibanHub>("/keijibanHub");

app.Run();
