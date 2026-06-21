using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using SareeProductionApp.Client.Pages;
using SareeProductionApp.Components;
using SareeProductionApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddScoped(sp => new HttpClient());
builder.Services.AddMudServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapControllers();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(SareeProductionApp.Client._Imports).Assembly);

using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var warp = context.YarnCategories.FirstOrDefault(c => c.Name == "Warp");
        if (warp != null) warp.Name = "Dyed Warp";

        var weft = context.YarnCategories.FirstOrDefault(c => c.Name == "Weft");
        if (weft != null) weft.Name = "Dyed Weft";

        var cone = context.YarnCategories.FirstOrDefault(c => c.Name == "Cone");
        if (cone != null) cone.Name = "Dyed Cone";

        context.SaveChanges();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating default categories: {ex.Message}");
    }
}

app.Run();
