using System.Reflection;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Db.Deployment.Cli.Processors;
using DbUp;
using DbUp.Engine;
using DbUp.Support;

namespace Db.Deployment.Cli.Commands;

[Command("upgrade", Description = "Command to upgrade the database.")]
public sealed class UpgradeDatabaseCommand : ICommand
{
    [CommandOption("connectionString", 'c', Description = "The connectionString to pass in to use for the upgrade.", IsRequired = true)]
    public string ConnectionString { get; init; }
    
    [CommandOption("journalTable", 'j', Description = "The journal table for versioning.", IsRequired = true)]
    public string JournalTableName { get; init; }
    
    [CommandOption("htmlReportPath", 'r', Description = "Use this option to include a report of the DDL/DML that was run during this process.")]
    public string? HtmlReportPath { get; init; }
    
    [CommandOption("preview", 'p', Description = "Use this option to just generate a preview report of what scripts will be run.")]
    public bool IsPreview { get; init; }
    
    public async ValueTask ExecuteAsync(IConsole console)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        var executingAssemblyName = executingAssembly.GetName().Name;
        var upgradeEngine = DeployChanges.To
            .SqlDatabase(ConnectionString)
            .JournalToSqlTable("dbo", JournalTableName)
            .WithScriptsEmbeddedInAssembly(executingAssembly,
                script => script.StartsWith($"{executingAssemblyName}.Database.DDL."),
                new SqlScriptOptions { ScriptType = ScriptType.RunOnce, RunGroupOrder = 1 })
            .WithScriptsEmbeddedInAssembly(executingAssembly,
                script => script.StartsWith($"{executingAssemblyName}.Database.DML."),
                new SqlScriptOptions { ScriptType = ScriptType.RunOnce, RunGroupOrder = 2 })
            .WithScriptsEmbeddedInAssembly(executingAssembly,
                script => script.StartsWith($"{executingAssemblyName}.Database.PostDeployment."),
                new SqlScriptOptions { ScriptType = ScriptType.RunOnce, RunGroupOrder = 3 })
            .WithPreprocessor(new NoUseStatementPreProcessor())
            .WithPreprocessor(new ValidateSqlPreProcessor())
            .WithTransactionPerScript()
            .LogToConsole()
            .Build();

        await console.Output.WriteLineAsync($"Is upgrade required: {upgradeEngine.IsUpgradeRequired()}");

        if (IsPreview)
        {
            var report = Path.Combine(HtmlReportPath, "UpgradeReport.html");
            await console.Output.WriteLineAsync($"Generating the report at {report}");
        }
        else
        {
            var result = upgradeEngine.PerformUpgrade();
            if (result.Successful)
            {
                console.ForegroundColor = ConsoleColor.Green;
                await console.Output.WriteLineAsync("Success!");
            }
            else
            {
                console.ForegroundColor = ConsoleColor.Red;
                await console.Output.WriteLineAsync(result.Error.Message);
                await console.Output.WriteLineAsync("Upgrade failed. Please read the exception message.");
            }
        }
    }
}