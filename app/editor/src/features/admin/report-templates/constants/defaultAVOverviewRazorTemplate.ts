export const defaultAVOverviewSubjectRazorTemplate = `@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ReportEngineAVOverviewModel>
@using TNO.TemplateEngine
Daily Today's News Online Media Overview - @($"{Instance.PublishedOn:dddd, MMMM dd, yyyy}")`;

export const defaultAVOverviewBodyRazorTemplate = `@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ReportEngineAVOverviewModel>
@using System
@using System.Linq
@using TNO.Entities

<style>
  .section {
    padding-bottom: 1rem;
  }
  .section-header {
    background-color: black;
    color: white;
    text-align: center;
    font-weight: bold;

  }
  table {
    border: solid 1px black;
    width: 100%;
  }
  td {
    border: solid 1px;
    padding: 0 1rem;
  }
  td.head {
    font-weight: bold;
  }
  .small {
    text-align: center;
    width: 50px;
  }
</style>
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
  <div class="section">
    <div class="section-header">
      @section.Name @section.StartTime hr
    </div>
    <table>
      <thead>
        <tr>
          <td class="small head">Placement</td>
          <td class="small head">Time</td>
          <td class="head">Story</td>
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
        var summary = String.IsNullOrEmpty(item.Summary) ? "---" : item.Summary.Replace("\n", "<br/>");
        <tr>
          <td class="small">@placement</td>
          <td class="small">@time</td>
          <td>
            @if (item.ContentId.HasValue)
            {
              <a href="@($"{ViewContentUrl}{item.ContentId}")">@summary</a>
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
