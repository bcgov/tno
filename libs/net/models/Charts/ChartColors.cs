namespace TNO.API.Models.Charts;

/// <summary>
/// ChartColors static class, provides the colours Chart.js datasets use. Shared by the Razor chart
/// templates and the aggregation chart data so that both produce the same output.
/// </summary>
public static class ChartColors
{
    /// <summary>
    /// Provides a javascript lambda function that will apply the correct colour in the sequence based on the value in the dataset.
    /// </summary>
    /// <param name="colors"></param>
    /// <returns></returns>
    public static string GetSentimentColorScript(string[]? colors)
    {
        var positive = colors?.Length > 0 ? colors[0] : "green";
        var neutral = colors?.Length > 1 ? colors[1] : "gold";
        var negative = colors?.Length > 2 ? colors[2] : "red";

        return "\"(ctx,options) => {{ " +
            "const index = ctx.dataIndex; " +
            "const value = ctx.dataset.data[index]; " +
            $"if (typeof value === 'number') return value === null ? null : value > 0 ? '{positive}' : value === 0 ? '{neutral}' : '{negative}'; " +
            "}}\"";
    }

    /// <summary>
    /// Extract the color for the specified dataset, or base the color the sentiment value.
    /// </summary>
    /// <param name="colors"></param>
    /// <param name="index">If -1 it will return all colours in the array.</param>
    /// <param name="dataset"></param>
    /// <param name="datasetName"></param>
    /// <param name="datasetValueProp"></param>
    /// <returns>A JSON fragment.</returns>
    public static string? GetColors(string[]? colors, int index, string dataset, string datasetName, string datasetValueProp)
    {
        if (dataset == "" && datasetValueProp == "sentiment")
        {
            return GetSentimentColorScript(colors);
        }

        if (dataset == "sentiment" || dataset == "sentimentSimple")
        {
            // A chart with a dataset for each sentiment should choose the color associated with the sentiment value.
            if (colors == null || colors.Length < 3)
                colors = new[] { "green", "gold", "red" };

            if (int.TryParse(datasetName, out int value))
                return $"[\"{(value > 0 ? colors[0] : value == 0 ? colors[1] : colors[2])}\"]";
            else
            {
                return $"[\"{datasetName switch
                {
                    "Positive" => colors[0],
                    "Neutral" => colors[1],
                    "Negative" => colors[2],
                    _ => colors[0],
                }}\"]";
            }
        }

        // An empty list is the same as no list. Chart.js draws a colour it cannot find in black, so
        // the chart is given no colour and keeps the ones Chart.js picks, as the chart editor does.
        if (colors == null || colors.Length == 0) return "null";

        // This will return a colour for each value in the dataset.
        if (index < 0) return $"[{String.Join(",", colors.Select(c => $"\"{c}\""))}]";

        // Use the index of the dataset to pick the colour.
        var length = colors.Length;
        var position = index < length ? index : index % length;
        return $"[\"{colors[position]}\"]";
    }
}
