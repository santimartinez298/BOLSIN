using Bolsines.Components;
using Bolsines.Persistencia;
using Microsoft.Data.Sqlite;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddMudServices();

// ----- Persistencia -----
// Se lee la cadena de conexión de appsettings.json y, si la ruta del archivo
// .db es relativa, se la ubica en la carpeta del proyecto (ContentRootPath).
var cadenaConexion = new SqliteConnectionStringBuilder(
    builder.Configuration.GetConnectionString("Bolsines") ?? "Data Source=bolsines.db"
);
if (!Path.IsPathRooted(cadenaConexion.DataSource))
{
    cadenaConexion.DataSource = Path.Combine(
        builder.Environment.ContentRootPath,
        cadenaConexion.DataSource
    );
}
ContextoBolsines.CadenaConexion = cadenaConexion.ToString();

var app = builder.Build();

// Crea la base de datos con sus tablas y datos iniciales si todavía no existe.
using (var contexto = new ContextoBolsines())
{
    contexto.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for
    // production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
