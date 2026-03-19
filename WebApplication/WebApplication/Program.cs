using WebApplication;

Microsoft.AspNetCore.Builder.WebApplication app = ProgramHostBuilder.CreateHostBuilder(args).Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}

app.MapControllers();


app.Run();
