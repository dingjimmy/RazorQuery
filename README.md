# Razor Query

## Built With

- [ASP.NET Core Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-9.0) (.NET 9)

## Overview

Razor Query is a .NET library with an aim to simplify the retrieval and modification of remote data in Razor
Components; it's designed to be easy to use and integrates seamlessly with Blazor's component model. While 
it takes inspriation from many places, the original concept was to provide a ReactQuery like experience 
to .NET developers using Blazor. 

 
## Local Development

To quickly set up and use in your local development environment, follow these steps:

1. **Prerequisites**
   - [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
   - [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (or your preferred IDE)

2. **Clone the repository**

3. **Build and Run**

   - Open the solution in Visual Studio 2022.
   - Restore NuGet packages.
   - Build the project.
   - Run unit tests to ensure everything is working correctly.
   - Start the sample application to see Razor Query in action.


## Usage

Example of using RazorQuery in a Rlazor Component

``` razor

@page "/todo"

<h1>Todo List...</h1>

@if (searchQuery.Status == QueryStatus.Pending)
{
    <p>Loading...</p>
}
else if (searchQuery.Status == QueryStatus.Error)
{
    <p class="text-danger">An error occured while loading the list.</p>
}
else if (searchQuery.Status == QueryStatus.Success)
{
    <ul class="list-group">
        @foreach (var item in searchQuery.Data)
        {
            <li class="list-group-item">
                <span>@item.Title</span>
            </li>
        }
    </ul>
}

@code 
{
    public string SearchText { get; set; } = string.Empty;


    private Query<ToDoListContent, string> searchQuery = new (async (searchText, ctx) =>
    {
        var httpClient = ctx.HttpClient;

        var response = await httpClient.GetAsync($"https://jsonplaceholder.typicode.com/todos");

        var item = await response.Content.ReadFromJsonAsync<ToDoListContent>();

        return item ?? new ToDoListContent();
    });


    protected override async Task OnInitializedAsync()
    {
        await searchQuery.Execute(SearchText);
    }
}
```

## License

This project is licensed under the [MIT License.](./LICENSE)