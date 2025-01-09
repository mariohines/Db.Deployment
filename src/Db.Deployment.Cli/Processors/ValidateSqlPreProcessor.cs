using DbUp.Engine;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Db.Deployment.Cli.Processors;

public sealed class ValidateSqlPreProcessor : IScriptPreprocessor
{
    public string Process(string contents)
    {
        var parser = new TSql160Parser(false, SqlEngineType.SqlAzure);
        parser.Parse(new StringReader(contents), out var errorSet);

        if (errorSet.Count > 0)
        {
            throw new AggregateException(errorSet.Select(error => new Exception(error.Message)));
        }

        return contents;
    }
}