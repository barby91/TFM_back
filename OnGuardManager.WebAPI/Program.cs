using Microsoft.EntityFrameworkCore;
using NLog.Web;
using onGuardManager.Bussiness.IService;
using onGuardManager.Bussiness.Service;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Data.Repository;
using onGuardManager.Logger;
using System.Text;
using onGuardManager.Models.Entities;

try
{
	LogClass.WriteLog(ErrorWrite.Info, "Se inicia el back");

	var builder = WebApplication.CreateBuilder(args);

	builder.Services.AddCors(options =>
	{
#if DEBUG
		options.AddPolicy(name: "AllowOrigin",
						  policy =>
						  {
							  policy.WithOrigins("https://localhost:44351", "http://localhost:4200")
									.AllowAnyHeader()
									.AllowAnyMethod();
						  });
#else
		options.AddPolicy(name: "AllowOrigin",
						  policy =>
						  {
							  policy.WithOrigins("https://main.dvoyy061ycswa.amplifyapp.com")
									.AllowAnyHeader()
									.AllowAnyMethod();
						  });
#endif
	});

	builder.Logging.ClearProviders();
	builder.Host.UseNLog();

	// Add services to the container.
	builder.Services.AddControllers();
	// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
	builder.Services.AddEndpointsApiExplorer();
	builder.Services.AddSwaggerGen(options =>
	{
		options.CustomSchemaIds(type => type.ToString());
	});

	builder.Services.AddControllersWithViews();
	
	builder.Services.AddDbContext<OnGuardManagerContext>(async opciones =>
	{
#if DEBUG
		opciones.UseSqlServer(builder.Configuration.GetConnectionString("cadenaSQLDev"));
#else
		opciones.UseSqlServer(builder.Configuration.GetConnectionString("cadenaSQL"));
#endif
	});

	#region inyección de repositorios
	builder.Services.AddScoped<IUserRepository<User>, UserRepository>();
	builder.Services.AddScoped<ISpecialtyRepository<Specialty>, SpecialtyRepository>();
	builder.Services.AddScoped<IRolRepository<Rol>, RolRepository>();
	builder.Services.AddScoped<ILevelRepository<Level>, LevelRepository>();
	builder.Services.AddScoped<IPublicHolidayRepository<PublicHoliday>, PublicHolidayRepository>();
	builder.Services.AddScoped<IAskedHolidayRepository<AskedHoliday>, AskedHolidayRepository>();
	builder.Services.AddScoped<IHolidayStatusRepository<HolidayStatus>, HolidayStatusRepository>();
	builder.Services.AddScoped<IUnityRepository<Unity>, UnityRepository>();
	builder.Services.AddScoped<IDayGuardRepository<DayGuard>, DayGuardRepository>();
	#endregion

	#region inyección de servicios
	builder.Services.AddScoped<IUserService, UserService>();
	builder.Services.AddScoped<ISpecialtyService, SpecialtyService>();
	builder.Services.AddScoped<IRolService, RolService>();
	builder.Services.AddScoped<ILevelService, LevelService>();
	builder.Services.AddScoped<IPublicHolidayService, PublicHolidayService>();
	builder.Services.AddScoped<IAskedHolidayService, AskedHolidayService>();
	builder.Services.AddScoped<IHolidayStatusService, HolidayStatusService>();
	builder.Services.AddScoped<IUnityService, UnityService>();
	builder.Services.AddScoped<IDayGuardService, DayGuardService>();
	#endregion

	var app = builder.Build();

	// Configure the HTTP request pipeline.
	app.UseSwagger();
	app.UseSwaggerUI();
	
	app.UseHttpsRedirection();

	app.UseCors("AllowOrigin");

	app.UseAuthorization();

	app.MapControllers();

	app.Run();
}catch(Exception ex)
{
	StringBuilder sb = new StringBuilder();
	sb.AppendFormat("Error en el back: {0}", ex.ToString());
	LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
}
finally
{
	LogClass.FlushNLog();
}