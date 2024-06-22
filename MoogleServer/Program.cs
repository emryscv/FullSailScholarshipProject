using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MoogleEngine;
using MoogleEngine.Tools;
using System.Diagnostics;
using System.Security.Principal;

public class Program
{
    public static VectorModel Model { get; private set; }
    public static Document[] Corpus { get; private set; }
    public static Dictionary<string, HashSet<string>> SynonymsDictionary { get; private set; }
    public static HashSet<string> StopWords { get; private set; }

    public static void Main(string[] args)
    {
        Stopwatch crono = new();
        crono.Start();

        StopWords = Preprocessing.LoadStopWords();
        Corpus = Preprocessing.LoadDocuments(StopWords);
        SynonymsDictionary = Preprocessing.LoadAndCreateSynonymsDictionary(StopWords);
        Model = new VectorModel(Corpus);

        crono.Stop();
        Console.WriteLine((double)crono.ElapsedMilliseconds / 1000);

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();

        var app = builder.Build();

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
    }
}