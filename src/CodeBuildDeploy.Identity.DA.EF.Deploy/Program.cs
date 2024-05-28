using CodeBuildDeploy.Identity.DA.EF.Deploy.DI;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.ConfigureApiServices();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.Run();
