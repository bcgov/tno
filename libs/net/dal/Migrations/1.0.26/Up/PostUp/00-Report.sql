DO $$
BEGIN

UPDATE public."report" SET
  "settings" = regexp_replace(regexp_replace('{ "subject": "Morning Report - @System.DateTime.Now.ToString(\"dddd, MMMM d, yyyy\")" }', '[\r\t]+', '', 'g'), '[\n]+', '\\n', 'g')::jsonb
  , "template" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.TemplateModel>
@using System.Linq
@{
  var groups = @Content.GroupBy(
    c => $"{c.Source?.Name}{(!System.String.IsNullOrWhiteSpace(c.Source?.Code) ? $" ({c.Source?.Code})" : "")}",
    c => c,
    (k, c) => new { Key = k, Content = c }).OrderBy(c => c.Key);
  var content = @Content.ToArray();
}
<h2 id="top">Media Monitoring Insights & Analysis - Morning Report</h2>
<div style="color:red;">DO NOT FORWARD THIS EMAIL TO ANYONE</div>
<br/>
<div>@System.DateTime.Now.ToString("dddd, MMMM d, yyyy")</div>
<h3>Table of Contents</h3>

@foreach (var group in groups)
{
  <h4>@group.Key</h4>
  <ul>
  @foreach (var p in group.Content)
 {
    <li>@p.Headline</li>
  }
  </ul>
}

<hr/>
<a href="#top">top</a>
<h2>Front Pages</h2>

@for (var i = 0; i < content.Length; i ++)
{
    var item = content[i];
    var hasPrev = i > 0;
    var prev= hasPrev ? content[i-1].Id : 0;
    var hasNext = (i + 1) < content.Length ? true : false;
    var next = hasNext ? content[i+1].Id : 0;

  <hr/>
  <a href="#top">top</a>
  <a href="#item-@prev">previous</a>
  <a href="#item-@next">next</a>
  <h2 id="item-@item.Id">@item.Headline</h2>
}'
WHERE "name" = 'Morning Report';

END $$;
