export const TalkwalkerCollageTemplate = `@using System
@using System.Collections.Generic
@using System.Dynamic
@using System.Linq
@using TNO.TemplateEngine

@{
  var items = Data as IEnumerable<dynamic>;
  var maxRows = 10;

  if (items == null || !items.Any())
  {
      <p>No results available.</p>
      return;
  }

  @functions
  {
    public int GetInt(dynamic item, string key)
    {
      var dict = item as IDictionary<string, object>;
      if (dict != null && dict.ContainsKey(key) && int.TryParse(dict[key]?.ToString(), out int value)) return value;
      return 0;
    }

    public string? GetString(dynamic item, string key)
    {
      var dict = item as IDictionary<string, object>;
      if (dict != null && dict.ContainsKey(key)) return dict[key]?.ToString();
      return null;
    }

    public dynamic[] CreateImages(
      dynamic[] images,
      bool[,] grid,
      int startIndex,
      int qty,
      int startCol = 0,
      int startRow = 0,
      int colSpan = 1,
      int rowSpan = 1)
    {
      var list = new List<dynamic>();
      for (int i = 0; i < qty && startIndex < images.Length; i++)
      {
        var image = images[startIndex] as IDictionary<string, object>;
        list.Add(new {
          StartCol = startCol,
          StartRow = startRow,
          ColSpan = colSpan,
          RowSpan = rowSpan,
          Width = $"{colSpan * 50}px",
          Height = $"{rowSpan * 50}px",
          Img = image,
          Engagement = ReportExtensions.FormatInteger(image["engagement"] as String, true),
          Url = image["url"] as String,
        });

        // Identify which cells are occupied in the grid.
        for (int c = startCol; c < startCol + colSpan; c++)
          for (int r = startRow; r < startRow + rowSpan; r++)
            grid[c,r] = true;

        startRow += rowSpan;
        startIndex++;
      }
      return list.ToArray();
    }
  }

  // Sort images by engagement (descending)
  var images = items.OrderByDescending(i => GetInt(i, "engagement")).ToArray();

  // Grid setup, configure the size per item per column.
  var sizePerCol = new int[] {5,4,4,3,3,2,2,1,1};
  bool[,] occupied = new bool[sizePerCol.Sum(), maxRows];
  var placedList = new List<dynamic>();

  int startIndex = 0;
  int colIndex = 0;

  for (int sizeIndex = 0; sizeIndex < sizePerCol.Length; sizeIndex++)
  {
    int size = sizePerCol[sizeIndex];
    int maxItems = maxRows / size;
    // Give the first item the addition space.
    var firstItemRowSpan = maxRows % size > 0 ? (maxRows - (size * maxItems)) + size : size;
    if (firstItemRowSpan != size)
    {
      placedList.AddRange(CreateImages(images, occupied, placedList.Count, 1, colIndex, 0, size, firstItemRowSpan));
      placedList.AddRange(CreateImages(images, occupied, placedList.Count, maxItems - 1, colIndex, firstItemRowSpan, size, size));
    }
    else
      placedList.AddRange(CreateImages(images, occupied, placedList.Count, maxItems, colIndex, 0, size, firstItemRowSpan));

    colIndex += size;
  }
}

<style>
    .image-table td {
      border: 2px solid;
      border-color: transparent;
      padding: 0px;
      vertical-align: top;
      overflow: hidden; /* Crop overflow */
      position: relative; /* For centering */
    }
    .image-table img {
      object-fit: cover; /* Crop to fill the cell(s) */
      object-position: center; /* Center the image */
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%); /* Center precisely */
      display: block;
    }
    .image-table {
      border-collapse: collapse;
      border: none;
    }
</style>

<table class="image-table">
    @for (int r = 0; r < maxRows; r++)
    {
        <tr>
            @for (int c = 0; c < sizePerCol.Sum(); c++)
            {
                var placed = placedList.FirstOrDefault(p => p.StartRow == r && p.StartCol == c);
                if (placed != null)
                {
                    <td rowspan="@placed.RowSpan" colspan="@placed.ColSpan" style="width: @placed.Width; height: @placed.Height;">
                      <a href="@placed.Url" target="_blank">
                        <img src="@GetString(placed.Img, "images.url")" alt="Engagement: @GetInt(placed.Img, "engagement")" style="width: @placed.Width; height: @placed.Height;" />
                        <div style="color: white; position: absolute; background-color: rgba(0,0,0,0.5); width: 100%; bottom: 0; text-align: center;">@placed.Engagement</div>
                      </a>
                    </td>
                }
                else if (occupied[c, r])
                {
                    <!-- Covered by span; output nothing -->
                }
                else
                {
                    <td style="width: 50px; height: 50px;"></td> <!-- Empty cell -->
                }
            }
        </tr>
    }
</table>
`;
