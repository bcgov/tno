export const defaultDateRazorTemplate = `@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
@using System
@using System.Linq
@using TNO.Entities
@{
 // Get all dates in data
  var dates = Content
    .GroupBy(c => c.PublishedOn.HasValue ? new DateTime(c.PublishedOn!.Value.Year, c.PublishedOn!.Value.Month, c.PublishedOn!.Value.Day) : DateTime.MinValue)
    .OrderBy(g=> g.Key)
    .ToArray();

  // Get all groups in data
  var groups = Content
    .GroupBy(
      c => Settings.GroupBy switch {
        "product" => c.Product?.Name,
        "contentType" => c.ContentType.ToString(),
        "byline" => c.Byline,
        "series" => c.Series?.Name,
        "sentiment" => c.TonePools.FirstOrDefault()?.Value.ToString(),
        "source" => c.OtherSource,
        _ => c.OtherSource
      },
      c => c,
      (k, c) => new { Label = k, Group = c.GroupBy(c => c.PublishedOn.HasValue ? new DateTime(c.PublishedOn!.Value.Year, c.PublishedOn!.Value.Month, c.PublishedOn!.Value.Day) : DateTime.MinValue) })
    .OrderBy(g=> g.Label)
    .ToArray();
}
{
  "labels": [@String.Join(",", dates.Select(c => $"\\"{c.Key:dd-MMM}\\""))],
  "datasets": [
    @for (var i = 0; i < groups.Length; i++)
    {
      @($"{{ \\"label\\": \\"{groups[i].Label}\\", \\"data\\": [{@String.Join(",", dates.Select(d => groups[i].Group.Where(g => g.Key == d.Key).Count()))} ] }}")
      if (i < groups.Length-1) @(",")
    }
   ]
}

`;

export const defaultCountRazorTemplate = `@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
@using System
@using System.Linq
@using TNO.Entities
@{
  var group = Settings.GroupBy switch
  {
    "product" => Content.GroupBy(c => c.Product?.Name).OrderBy(group => group.Key),
    "contentType" => Content.GroupBy(c => c.ContentType.ToString()).OrderBy(group => group.Key),
    "byline" => Content.GroupBy(c => c.Byline).OrderBy(group => group.Key),
    "series" => Content.GroupBy(c => c.SeriesId.ToString()).OrderBy(group => group.Key),
    "sentiment" => Content.GroupBy(c => c.TonePools.FirstOrDefault()?.Value.ToString() ?? "0").OrderBy(group => group.Key),
    "source" => Content.GroupBy(c => c.OtherSource).OrderBy(group => group.Key),
    _ =>  Content.GroupBy(c => c.OtherSource).OrderBy(group => group.Key),
  };
}
{
  "labels": [@String.Join(",", group .Select(c => $"\\"{c.Key}\\""))],
  "datasets": [
    {
      "label": "Totals",
      "data":  [@String.Join(", ", group .Select(c => c.Count()))]
    }
   ]
}`;
