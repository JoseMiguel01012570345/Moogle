using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MoogleEngine;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
var app = builder.Build();

TF_IDF1.VerifyPossibleNull();
Reading.WordsCorpus();
TF_IDF1.WeightDatabase();
TF_IDF1.DocCopy();
Synonimus.MySynonimus();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
