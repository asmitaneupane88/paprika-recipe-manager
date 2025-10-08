using System;
using System.Collections.Generic;   //For list<Recipe>
using System.Net.Http;              //For HttpClient
using System.Text.Json;
using System.Threading.Tasks;


namespace RecipeApp.Services;

public class ApiControl
{
    public string Name { get; set; } = string.Empty;
    public string Ingredients { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string ImageUlr { get; set; } = string.Empty;
}
