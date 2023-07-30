using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

//creamos la politica de autorizacion, esto para aplicar el atributo [Authorize] a todos los controladores
var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder()
    //requreriremos la auternticacion de los usuarios
    .RequireAuthenticatedUser()
    //construiremos la politica de autenticacion de usuarios
    .Build();

// Add services to the container.
builder.Services.AddControllersWithViews( opciones =>
{
    //buscamos la opcion de filtro
    opciones.Filters
    //agregamos la politica que cosntruimos
        .Add(new AuthorizeFilter(politicaUsuariosAutenticados));
});
//configuramos el servicio, con su interfaces y su clase
builder.Services.AddTransient<IRepositorioTiposCuentas, RepositorioTiposCuentas>();
builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();
builder.Services.AddTransient<IReposotorioCuentas, RepositorioCuentas>();
builder.Services.AddTransient<IRepositorioCategorias, RepositorioCategorias>();
builder.Services.AddTransient<IRepositorioTransacciones, RepositorioTransacciones>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IServicioReportes, ServicioReportes>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddTransient<IRepositorioUsuarios, RepositorioUsuarios>();
//configuracion de iuserstore y identity
builder.Services.AddTransient<IUserStore<Usuario>, UsuarioStore>();

//***************identity
//builder.Services.AddIdentityCore<Usuario>(opciones =>
//{
//    opciones.Password.RequireUppercase = false;
//    opciones.Password.RequireNonAlphanumeric = false;
//    opciones.Password.RequireDigit = false;
//    opciones.Password.RequireLowercase = false;
//});
//*****Incrementamos los metodos tradusidos a nuestro idioma
builder.Services.AddIdentityCore<Usuario>().AddErrorDescriber<MensajesDeErrorIdentity>();

//autenticacion y creacion de la cooki
builder.Services.AddTransient<SignInManager<Usuario>>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
}).AddCookie(IdentityConstants.ApplicationScheme, opciones =>
{
    //agregamos nuestro propio path de configuracion de login
    opciones.LoginPath = "/usuarios/login";
    //ademas podemso cambair nuestro link para el logout
    opciones.LogoutPath = "/usuarios/login";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Home}/{action=Index}/{id?}");
    //pattern: "{controller=Transacciones}/{action=Index}/{id?}");
    pattern: "{controller=Usuarios}/{action=Login}/");

app.Run();
