export const defaultAVOverviewSubjectRazorTemplate = `@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ReportEngineAVOverviewModel>
@using TNO.TemplateEngine
Daily Today's News Online Media Overview - @($"{Instance.PublishedOn:dddd, MMMM dd, yyyy}")`;

export const defaultAVOverviewBodyRazorTemplate = `@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ReportEngineAVOverviewModel>
@using System
@using System.Linq
@using TNO.Entities

<h2 id="top">Daily Today's News Online Media Overview</h2>
<h3>@($"{Instance.PublishedOn:dddd, MMMM dd, yyyy}")</h3>
<br/>
@if (Instance.Sections.Count() == 0)
{
  <p>There is no content in this report.</p>
}
@foreach (var section in Instance.Sections)
{
  var itemIndex = 0;
  var itemCount = section.Items.Count(i => i.ItemType == AVOverviewItemType.Story);
  <div style="padding-bottom: 1rem;">
    <div style="background-color: black; color: white; text-align: center; font-weight: bold;">
      @section.Name @section.StartTime hr
    </div>
    <table style="border: solid 1px black; width: 100%;">
      <thead>
        <tr>
          <td style="text-align: center; width: 50px; border: solid 1px; padding: 0 1rem; font-weight: bold;">Placement</td>
          <td style="text-align: center; width: 50px; border: solid 1px; padding: 0 1rem; font-weight: bold;">Time</td>
          <td style="border: solid 1px; padding: 0 1rem; font-weight: bold;">Story</td>
        </tr>
      </thead>
      @foreach (var item in section.Items)
      {
        var placement = item.ItemType switch
        {
          AVOverviewItemType.Intro => "Intro",
          AVOverviewItemType.Story => itemIndex  == 1 ? "Top Story" : $"{itemIndex}/{itemCount}",
          AVOverviewItemType.Ad => "",
          _ => $"{item.ItemType}",
        };
        var time = item.ItemType switch
        {
          AVOverviewItemType.Ad => "",
          _ => $"{item.Time}",
        };
        var summary = String.IsNullOrEmpty(item.Summary) ? "---" : item.Summary;
        <tr>
          <td style="text-align: center; border: solid 1px;">@placement</td>
          <td style="text-align: center; border: solid 1px;">@time</td>
          <td style="border: solid 1px; padding: 0 1rem;">
            @if (item.ContentId.HasValue)
            {
              <a href="@($"{ViewContentUrl}{item.ContentId}")">@item.Summary</a>
            }
            else
            {
              @summary
            }
          </td>
        </tr>
        @{
          itemIndex++;
        }
      }
    </table>
  </div>
}
`;
