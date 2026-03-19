using WebApplication.Services;

namespace WebApplication;

public static class ProgramHostBuilder
{
    public static WebApplicationBuilder CreateHostBuilder(string[] args)
    {
        WebApplicationBuilder builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

        AddApiFramework(builder);
        AddLogging(builder);
        AddEmployeeServices(builder);

        InitializeApp(builder);

        return builder;
    }

    //  API Framework (Controllers + Swagger)
    private static void AddApiFramework(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    //  Logging
    private static void AddLogging(WebApplicationBuilder builder)
    {
        builder.Logging.AddConsole();
    }

    // Employee Services (Proxy + DB)
    private static void AddEmployeeServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IEmployeeService, EmployeeService>();
        builder.Services.Decorate<IEmployeeService, EmployeeCacheProxy>();
    }

    //Initialize
    private static void InitializeApp(WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<DatabaseInitializer>();
    }
}
