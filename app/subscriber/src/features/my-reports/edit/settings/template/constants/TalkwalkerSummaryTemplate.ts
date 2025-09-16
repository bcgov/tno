export const TalkwalkerSummaryTemplate = `@using System
@using System.Linq
@using System.Text.Json
@{
  var replaceHeader = new System.Text.RegularExpressions.Regex(@"\\*\\*(.*?)\\*\\*");
  var replaceBold = new System.Text.RegularExpressions.Regex(@"\\*(.{1,25})\\*");
  var replaceBadFormatting = new System.Text.RegularExpressions.Regex(@"\\*");
  var replaceNewLine = new System.Text.RegularExpressions.Regex(@"\\n");
  var json = Data as JsonDocument;
}

@foreach (var element in json.RootElement.EnumerateArray())
{
  var summary = element.GetProperty("summary").GetString();
  summary = replaceHeader.Replace(summary, "<h2 style=\\"margin-top:1rem;\\">$1</h2>");
  summary = replaceBold.Replace(summary, "<b>$1</b>");
  summary = replaceBadFormatting .Replace(summary, "");
  summary = replaceNewLine.Replace(summary, "");

  <div>@(summary)</div>
}`;
