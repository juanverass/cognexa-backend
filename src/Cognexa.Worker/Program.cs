using Cognexa.Application;
using Cognexa.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddApplication().AddInfrastructure(builder.Configuration);
await builder.Build().RunAsync();
