using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var nextId = 0;
var recipes = new ConcurrentDictionary<int, Recipe>();

app.MapGet("/recipes", () => recipes.Values);

app.MapPost("/recipes", (RecipeDto recipeDto) =>
{
    var incrementedId = Interlocked.Increment(ref nextId);

    var recipe = new Recipe
    {
        Id = incrementedId,
        Title = recipeDto.Title,
        Ingredients = recipeDto.Ingredients,
        Description = recipeDto.Description,
        ImageUrl = recipeDto.ImageUrl
    };
    if(!recipes.TryAdd(incrementedId, recipe)){
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
    return Results.Created($"/recipes/{incrementedId}", recipe);
});

app.MapDelete("/recipes/{id}", (int id) =>
{
    if(recipes.TryRemove(id, out var recipe)){
        return Results.Ok(recipe);
    }
    return Results.NotFound();
});

app.MapGet("/recipes/filteredRecipesByTitle",(string? title)=>
{
    if(title is null){
        return Results.BadRequest();
    }
    var filteredRecipes = recipes.Values.Where(recipe => recipe.Title.Contains(title));
    return Results.Ok(filteredRecipes);
});

app.MapGet("/recipes/filteredRecipesByIngredient",(string? ingredient)=>
{
    if(ingredient is null){
        return Results.BadRequest();
    }
    var filteredRecipes = recipes.Values.Where(recipe => recipe.Ingredients?.Any(ing => ing.Name.Contains(ingredient)) ?? false);
    return Results.Ok(filteredRecipes);
});

app.MapPut("/recipes/replace/{id}", (int id, RecipeDto recipeDto) =>
{
    if(recipes.TryGetValue(id, out var recipe)){
        recipe.Title = recipeDto.Title;
        recipe.Ingredients = recipeDto.Ingredients;
        recipe.Description = recipeDto.Description;
        recipe.ImageUrl = recipeDto.ImageUrl;
        return Results.Ok(recipe);
    }
    return Results.NotFound();
});

app.Run();

class Recipe
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public List<Ingredient>? Ingredients { get; set; }
    public String Description { get; set; } = "";
    public string? ImageUrl { get; set; }

}

class Ingredient
{
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "";
    public int Quantity { get; set; } 
}


record RecipeDto(string Title, List<Ingredient>? Ingredients,string Description, string? ImageUrl);