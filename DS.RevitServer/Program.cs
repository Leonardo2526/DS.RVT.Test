using Autodesk.Revit.DB;
using DataLib;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


var list = new List<XYZ>
{
    new XYZ(0, 0, 0)
};

app.MapGet("", () => new Response(list));
//app.MapGet("/", () => "Hello World!");

app.Run();
