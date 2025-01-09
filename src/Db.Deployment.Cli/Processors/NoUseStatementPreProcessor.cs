using System.Text.RegularExpressions;
using DbUp.Engine;

namespace Db.Deployment.Cli.Processors;

public sealed partial class NoUseStatementPreProcessor : IScriptPreprocessor
{
    private const string SearchExpression = "^use \\w+;?$";
    
    public string Process(string contents)
    {
        var regex = SearchExpressionRegex();
        var lineSet = contents.Split(Environment.NewLine).ToList();
        var useStatementIndex = lineSet.FindIndex(s => regex.IsMatch(s));

        if (useStatementIndex != 0) return contents;
        var firstGoIndex = lineSet.FindIndex(useStatementIndex,
            s => s.Contains("go", StringComparison.InvariantCultureIgnoreCase));

        return string.Join(Environment.NewLine, lineSet.Where((_, i) => i != useStatementIndex && i != firstGoIndex));
    }

    [GeneratedRegex(SearchExpression, RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-US")]
    private static partial Regex SearchExpressionRegex();
}