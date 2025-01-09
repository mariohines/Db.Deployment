// See https://aka.ms/new-console-template for more information

using CliFx;
using Db.Deployment.Cli.Commands;
using Microsoft.Extensions.DependencyInjection;

var provider = BuildServiceProvider();
return await new CliApplicationBuilder()
    .UseTypeActivator(t => provider.GetService(t))
    .AddCommandsFromThisAssembly()
    .Build()
    .RunAsync(args);

static IServiceProvider BuildServiceProvider()
{
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddTransient<UpgradeDatabaseCommand>();

    return serviceCollection.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
}