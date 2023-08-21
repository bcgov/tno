export const defaultRazorTemplate = `@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ReportEngineContentModel>
@using System
@using System.Linq
@using TNO.Entities
@{
  var pageBreak = Settings.Sections.UsePageBreaks ? "page-break-after: always;" : "";
}
<h2 id="top">Report Title</h2>
<div style="color:red;">DO NOT FORWARD THIS EMAIL TO ANYONE</div>
<br/>
@if (Content.Count() == 0)
{
  <p>There is no content in this report.</p>
}
@foreach (var section in Sections)
{
  if (section.Value.IsEnabled && section.Value.Content.Count() > 0 || !Settings.Sections.HideEmpty)
  {
    <div style="@pageBreak">
      <h3>@section.Value.Settings.Label</h3>
      <p>@section.Value.Description</p>
      @if (section.Value.Settings.ShowContent)
      {
        <ul>
          @foreach (var content in section.Value.Content)
          {
            <li>@content.Headline @Settings.Headline.ShowSource</li>
          }
        </ul>
      }
    </div>
  }
}

`;
