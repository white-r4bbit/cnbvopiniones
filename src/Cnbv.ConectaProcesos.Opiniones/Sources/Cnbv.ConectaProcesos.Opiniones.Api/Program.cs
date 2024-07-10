using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Cnbv.ConectaProcesos.Opiniones.Business;
using Cnbv.ConectaProcesos.Opiniones.Common;
using Cnbv.ConectaProcesos.Opiniones.Data.DatabaseConectaProcesos;
using Cnbv.ConectaProcesos.Opiniones.Data.Implementations;
using Cnbv.ConectaProcesos.Opiniones.Data.Interfaces;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

/**************************************************************/
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "Solicitud de Opiniones", Version = "v1" });

  // Set the comments path for the Swagger JSON and UI.
  var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
  var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
  c.IncludeXmlComments(xmlPath);
});
/**************************************************************/

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//// Configuración del acceso a los secrets -------------------------------------

//// ----------------------------------------------------------------------------

string kvUrl = builder.Configuration["KVConfig:KVUrl"];
string kvTenantId = builder.Configuration["KVConfig:TenantId"];
string kvClientId = builder.Configuration["KVConfig:ClientId"];
string kvClientSecretId = builder.Configuration["KVConfig:ClientSecretId"];


var credential = new ClientSecretCredential(kvTenantId, kvClientId, kvClientSecretId);

builder.Configuration.AddAzureKeyVault(
      new Uri(kvUrl),
      credential);

var client = new SecretClient(
        new Uri(kvUrl),
       credential);

builder.Services.AddSingleton(new SecretClient(new Uri(kvUrl), credential));
builder.Services.AddSingleton<KeyVaultService>();

////-----------------------------------------------------------------------------
var appInsightsKey = client.GetSecret("CNBV--PortalGestion--ApplInsightsKey");
var telemetryConfig = new TelemetryConfiguration(appInsightsKey.Value.Value);

builder.Services.Configure<TelemetryConfiguration>(cfg =>
{
  cfg.InstrumentationKey = telemetryConfig.InstrumentationKey;
});

builder.Services.AddSingleton<AppInsightsService>();
builder.Services.AddApplicationInsightsTelemetry();
////-----------------------------------------------------------------------------

//// -----------------
var dbConnectionStringSecret = client.GetSecret("CNBV--ConectaProcesos--DbConnection");

builder.Services.AddScoped<IOpinionesBusiness, OpinionesBusiness>();

builder.Services.AddDbContext<ConectaProcesosContext>(options =>
{
  options.UseSqlServer("Server=172.16.132.77\\SQLDES3CS;Initial Catalog=ConectaProcesosRefactor;User Id=idgenConectaProcesos; Password=db*VqR_oebv18erbB0; Encrypt=False; TrustServerCertificate=False; ApplicationIntent=ReadWrite; MultiSubnetFailover=False; MultipleActiveResultSets=True");
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
//// -----------------

var app = builder.Build();

app.UseCors(builder =>
{
  builder
  .AllowAnyOrigin()
  .AllowAnyMethod()
  .AllowAnyHeader();
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Scaffold-DbContext "Server=SR21VIBD02\SQLDES3CS;User=idgenConectaProcesos;Password=db*VqR_oebv18erbB0;Database=ConectaProcesos;Integrated Security=false;Encrypt=false" Microsoft.EntityFrameworkCore.SqlServer -OutputDir DatabaseConectaProcesos -Tables Catalogo.TipoDocumentoEnum,Catalogo.TipoElementoEnum,Opinion.Opinion,Opinion.OpinionReceptor,Opinion.ArchivoOpinion -Force -NoOnConfiguring