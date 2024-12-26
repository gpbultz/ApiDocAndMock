using ApiDocAndMockCli;
using System.Text;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        string jsonFile = "";
        string outputPath = "";

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--json" && i + 1 < args.Length)
                jsonFile = args[i + 1];
            if (args[i] == "--output" && i + 1 < args.Length)
                outputPath = args[i + 1];
        }

        if (string.IsNullOrEmpty(jsonFile) || string.IsNullOrEmpty(outputPath))
        {
            Console.WriteLine("Usage: mockapi --json <schema.json> --output <path>");
            return;
        }

        var schema = JsonSerializer.Deserialize<SchemaDefinition>(File.ReadAllText(jsonFile));
        GenerateFullProject(schema, outputPath);
    }

    static void GenerateFullProject(SchemaDefinition schema, string outputPath)
    {
        Directory.CreateDirectory(outputPath);

        // Basic project folders
        Directory.CreateDirectory(Path.Combine(outputPath, "Endpoints"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Application"));

        // Generate Program.cs
        File.WriteAllText(Path.Combine(outputPath, "Program.cs"), GenerateProgramCs(schema));

        // Generate .csproj file
        File.WriteAllText(Path.Combine(outputPath, $"{Path.GetFileName(outputPath)}.csproj"), GenerateCsProj());

        // Inject Services and Middleware
        InjectBuilderExtensions(outputPath);
        InjectAppExtensions(outputPath);

        // Generate endpoints, commands, and queries
        foreach (var entity in schema.Entities)
        {
            GenerateEndpointExtension(entity, outputPath);
            GenerateCommandsQueriesResponses(entity, outputPath);
        }

        Console.WriteLine("✅ .NET 8 Project successfully generated!");
    }

    static string GenerateProgramCs(SchemaDefinition schema)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using Microsoft.AspNetCore.Builder;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");

        // Add endpoint namespaces
        foreach (var entity in schema.Entities)
        {
            sb.AppendLine($"using {entity.Name}Api.Endpoints;");
        }

        sb.AppendLine();
        sb.AppendLine("var builder = WebApplication.CreateBuilder(args);");
        sb.AppendLine("builder.Services.AddMockApiServices();");
        sb.AppendLine("var app = builder.Build();");
        sb.AppendLine("app.UseMockApi();");

        foreach (var entity in schema.Entities)
        {
            sb.AppendLine($"app.Map{entity.Name}Endpoints();");
        }

        sb.AppendLine("app.Run();");
        return sb.ToString();
    }

    static string GenerateCsProj()
    {
        return @"
<Project Sdk=""Microsoft.NET.Sdk.Web"">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""Microsoft.AspNetCore.OpenApi"" Version=""8.0.0"" />
    <PackageReference Include=""ApiDocAndMock"" Version=""1.0.0"" />
  </ItemGroup>
</Project>";
    }

    static void GenerateEndpointExtension(EntityDefinition entity, string projectPath)
    {
        string lowerEntity = entity.Name.ToLower();
        var outputPath = Path.Combine(projectPath, "Endpoints", $"{entity.Name}Endpoints.cs");

        var endpointCode = $@"
using ApiDocAndMock.Infrastructure.Extensions;
using ApiDocAndMock.Infrastructure.Mocking;
using Microsoft.AspNetCore.Mvc;
using {entity.Name}Api.Application;

namespace {entity.Name}Api.Endpoints
{{
    public static class {entity.Name}Endpoints
    {{
        public static void Map{entity.Name}Endpoints(this IEndpointRouteBuilder app)
        {{
            // Get All
            app.MapGet(""/{lowerEntity}"", ([FromServices] IMemoryDb db) =>
            {{
                var response = ApiMockDataFactory.CreateMockObject<Get{entity.Name}sResponse>(10);
                return Results.Ok(response);
            }})
            .Produces<Get{entity.Name}sResponse>(200)
            .WithMockResponse<Get{entity.Name}sResponse>();

            // Get By Id
            app.MapGet(""/{lowerEntity}/{{id:guid}}"", (Guid id) =>
            {{
                var response = ApiMockDataFactory.CreateMockObject<GetById{entity.Name}Response>();
                return Results.Ok(response);
            }})
            .GetMockFromMemoryDb<GetById{entity.Name}Response>()
            .Produces<Get{entity.Name}ByIdResponse>(200)
            .Produces(404);

            // Create
            app.MapPost(""/{lowerEntity}"", ([FromBody] Create{entity.Name}Command command) =>
            {{
                return Results.Created($""/{lowerEntity}/{{command.Id}}"", command);
            }})
            .CreateMockWithMemoryDb<Create{entity.Name}Command, GetById{entity.Name}Response, Create{entity.Name}Response>();

            // Update
            app.MapPut(""/{lowerEntity}/{{id:guid}}"", ([FromBody] Update{entity.Name}Command command) =>
            {{
                return Results.Ok(new Update{entity.Name}Response {{ Result = ""updated"" }});
            }})
            .UpdateMockWithMemoryDb<Update{entity.Name}Command, GetById{entity.Name}Response, Update{entity.Name}Response>();

            // Delete
            app.MapDelete(""/{lowerEntity}/{{id:guid}}"", () =>
            {{
                return Results.NoContent();
            }})
            .DeleteMockWithMemoryDb<GetById{entity.Name}Response, Delete{entity.Name}Response>();
        }}
    }}
}}";

        File.WriteAllText(outputPath, endpointCode);
        Console.WriteLine($"✔️ {entity.Name}Endpoints.cs created.");
    }

    static void GenerateCommandsQueriesResponses(EntityDefinition entity, string projectPath)
    {
        var outputPath = Path.Combine(projectPath, "Application", entity.Name);
        Directory.CreateDirectory(outputPath);

        var commonProperties = GenerateProperties(entity.Properties);

        // Create Command
        File.WriteAllText(Path.Combine(outputPath, $"Create{entity.Name}Command.cs"),
            GenerateCommand(entity.Name, commonProperties, "Create"));

        // Update Command
        File.WriteAllText(Path.Combine(outputPath, $"Update{entity.Name}Command.cs"),
            GenerateCommand(entity.Name, commonProperties, "Update"));

        // Responses
        File.WriteAllText(Path.Combine(outputPath, $"Get{entity.Name}ByIdResponse.cs"),
            GenerateResponse(entity.Name, commonProperties, "GetById"));

        File.WriteAllText(Path.Combine(outputPath, $"Get{entity.Name}sResponse.cs"),
            GenerateResponse(entity.Name, commonProperties, "GetAll"));
    }

    // Missing Method Implementations
    static string GenerateCommand(string entity, string properties, string action)
    {
        return $@"
namespace {entity}Api.Application
{{
    public class {action}{entity}Command
    {{
{properties}
    }}
}}";
    }
    static string GenerateResponse(string entity, string properties, string action)
    {
        return $@"
namespace {entity}Api.Application
{{
    public class {action}{entity}Response
    {{
{properties}
        public string Status {{ get; set; }} = ""Success"";
    }}
}}";
    }


    static string GenerateQuery(string entity, string action)
    {
        return $@"
namespace {entity}Api.Application
{{
    public class {action}{entity}Query
    {{
        public Guid Id {{ get; set; }}
    }}
}}";
    }

    static string GenerateProperties(Dictionary<string, string> properties)
    {
        var sb = new StringBuilder();
        foreach (var property in properties)
        {
            sb.AppendLine($"    public {property.Value} {property.Key} {{ get; set; }}");
        }
        return sb.ToString();
    }

    static void InjectBuilderExtensions(string projectPath)
    {
        var path = Path.Combine(projectPath, "ServiceExtensions.cs");
        if (File.Exists(path)) return;

        File.WriteAllText(path, @"
using Microsoft.Extensions.DependencyInjection;
using ApiDocAndMock.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddMockApiServices(this IServiceCollection services)
    {
        services.AddDocAndMock();
        services.AddMockAuthentication();
        services.AddMockSwagger(true, true);
        services.AddMemoryDb();
        return services;
    }
}");
    }

    static void InjectAppExtensions(string projectPath)
    {
        var path = Path.Combine(projectPath, "AppExtensions.cs");
        if (File.Exists(path)) return;

        File.WriteAllText(path, @"
using Microsoft.AspNetCore.Builder;

public static class AppExtensions
{
    public static IApplicationBuilder UseMockApi(this IApplicationBuilder app)
    {
        app.UseApiDocAndMock(true, true);
        app.UseSwagger();
        app.UseSwaggerUI();
        return app;
    }
}");
    }
}
