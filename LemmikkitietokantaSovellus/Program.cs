using Lemmikkitietokanta.Data; // Varmista, että tämä viittaa oikeaan DbContext-luokkaan
using Lemmikkitietokanta.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Korjaa tässä: vaihda Lemmikkitietokanta LemmikkiDbContext:ksi
builder.Services.AddDbContext<LemmikkiDbContext>(options =>
    options.UseSqlite("Data Source=../Lemmikkitietokanta/Lemmikkitietokanta.db",
        b => b.MigrationsAssembly("LemmikkitietokantaSovellus")));



builder.Services.AddHttpClient("LemmikkiApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:5001"); // API:n osoite
});

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
