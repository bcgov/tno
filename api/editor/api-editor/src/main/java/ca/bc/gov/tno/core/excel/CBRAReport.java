package ca.bc.gov.tno.core.excel;

import java.time.Duration;
import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.stream.Collectors;

import org.apache.poi.ss.usermodel.FillPatternType;
import org.apache.poi.ss.usermodel.HorizontalAlignment;
import org.apache.poi.ss.usermodel.IndexedColors;
import org.apache.poi.ss.util.CellRangeAddress;
import org.apache.poi.xssf.usermodel.XSSFCellStyle;
import org.apache.poi.xssf.usermodel.XSSFSheet;
import org.apache.poi.xssf.usermodel.XSSFWorkbook;

import ca.bc.gov.tno.dal.db.entities.Content;
import ca.bc.gov.tno.dal.db.entities.Series;
import ca.bc.gov.tno.dal.db.entities.User;
import ca.bc.gov.tno.dal.db.models.FilterCollection;
import ca.bc.gov.tno.dal.db.models.LogicalOperators;
import ca.bc.gov.tno.dal.db.services.interfaces.IActionService;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentService;
import ca.bc.gov.tno.dal.db.services.interfaces.IUserService;

public class CBRAReport {

  private final IContentService contentService;
  private final IUserService userService;
  private final IActionService actionService;
  private final XSSFWorkbook workbook = new XSSFWorkbook();
  private final XSSFSheet sheet = workbook.createSheet("CBRA");
  private final DateTimeFormatter dateFormat = DateTimeFormatter.ofPattern("MM-dd-yyyy");

  // Data Variables
  private Content[] content;
  private List<Content> talkRadio;
  private Map<Optional<Series>, List<Content>> talkRadioSeries;
  private ArrayList<Optional<Series>> talkRadioKeys;
  private List<Content> newsRadio;
  private ArrayList<String> newsRadioKeys;
  private List<Content> television;
  private Map<String, List<Content>> newsRadioSources;
  private ArrayList<Optional<Series>> televisionKeys;
  private Map<Optional<Series>, List<Content>> televisionSeries;
  private int maxRows;

  /**
   * Creates a new instance of a CBRAReport object, initializes with specified
   * parameters.
   *
   * @param contentService Content service.
   * @param userService    User service.
   * @param actionService  Action service.
   */
  public CBRAReport(final IContentService contentService, final IUserService userService,
      final IActionService actionService) {
    this.contentService = contentService;
    this.userService = userService;
    this.actionService = actionService;

    sheet.setColumnWidth(0, 6000);
    sheet.setColumnWidth(1, 4000);
  }

  private XSSFCellStyle createStyle(String font, short size, boolean bold, boolean wrap,
      HorizontalAlignment horizontalAlign) {

    var fontStyle = workbook.createFont();
    fontStyle.setFontName(font);
    fontStyle.setFontHeightInPoints((short) size);
    fontStyle.setBold(bold);

    var style = workbook.createCellStyle();
    // style.setFillForegroundColor(IndexedColors.LIGHT_BLUE.getIndex());
    // style.setFillPattern(FillPatternType.SOLID_FOREGROUND);
    style.setFont(fontStyle);
    style.setWrapText(wrap);
    style.setAlignment(horizontalAlign);

    return style;
  }

  public XSSFWorkbook generateReport(ZonedDateTime from, ZonedDateTime to) {
    var filter = new FilterCollection();
    filter.addFilter("updatedOn", LogicalOperators.GreaterThanOrEqual, from);
    filter.addFilter("updatedOn", LogicalOperators.LessThanOrEqual, to);
    var page = contentService.find(1, 10000, filter, null); // TODO: Asking for a page isn't ideal.

    // TODO: This is horrible, but hibernate is a mess. Need to make this more
    // performant.
    content = page.getItems().stream().map((c) -> {
      return contentService.findById(c.getId(), true).get();
    }).toArray(Content[]::new);

    talkRadio = Arrays.stream(content).filter(c -> c.getMediaType().getName().equals("Talk Radio")).toList();
    talkRadioSeries = talkRadio.stream()
        .collect(Collectors.groupingBy(c -> Optional.ofNullable((c.getSeries()))));
    talkRadioKeys = new ArrayList<Optional<Series>>(talkRadioSeries.keySet());

    newsRadio = Arrays.stream(content).filter(c -> c.getMediaType().getName().equals("News Radio")).toList();
    newsRadioSources = newsRadio.stream().collect(Collectors.groupingBy(Content::getSource));
    newsRadioKeys = new ArrayList<String>(newsRadioSources.keySet());

    television = Arrays.stream(content).filter(c -> c.getMediaType().getName().equals("Television")).toList();
    televisionSeries = television.stream()
        .collect(Collectors.groupingBy(c -> Optional.ofNullable((c.getSeries()))));
    televisionKeys = new ArrayList<Optional<Series>>(televisionSeries.keySet());

    int[] sizes = { talkRadioSeries.size(), newsRadioSources.size(), televisionSeries.size() };
    maxRows = Arrays.stream(sizes).max().getAsInt();

    addTitle(from, to);
    var lastRow = addTotalExcerptsByProgram(3);
    lastRow = addTotalRunningTimeByProgram(lastRow + 3);
    lastRow = addPercentOfTotalRunningTimeByProgram(lastRow + 3);
    lastRow = addPercentOfTotalRunningTime(lastRow + 3);
    lastRow = addSpacerRow(lastRow + 2);
    lastRow = addTotals(lastRow + 2);
    lastRow = addSpacerRow(lastRow + 2);
    lastRow = addStaffSummary(lastRow + 2, from, to);
    lastRow = addSpacerRow(lastRow + 2);
    addDatabaseEntries(lastRow + 2);

    return workbook;
  }

  private int addSpacerRow(int startRowIndex) {
    var style = workbook.createCellStyle();
    style.setFillForegroundColor(IndexedColors.LIGHT_BLUE.getIndex());
    style.setFillPattern(FillPatternType.SOLID_FOREGROUND);

    var row = sheet.createRow(startRowIndex);
    row.setRowStyle(style);

    return startRowIndex;
  }

  private void addTitle(ZonedDateTime from, ZonedDateTime to) {
    var localFrom = from.toLocalDateTime();
    var localTo = to.toLocalDateTime();

    var titleStyle = createStyle("Arial", (short) 16, true, false, HorizontalAlignment.CENTER);

    var titleRow = sheet.createRow(1);
    sheet.addMergedRegion(CellRangeAddress.valueOf("A2:E2"));
    var titleCell = titleRow.createCell(0);
    titleCell.setCellStyle(titleStyle);
    titleCell.setCellValue(
        String.format("CBRA Summary for %s to %s", dateFormat.format(localFrom), dateFormat.format(localTo)));
  }

  private int addTotalExcerptsByProgram(int startRowIndex) {
    var titleStyle = createStyle("Arial", (short) 16, true, false, HorizontalAlignment.CENTER);

    var totalExcerptsRow = sheet.createRow(startRowIndex);
    sheet.addMergedRegion(CellRangeAddress.valueOf("A4:E4"));
    var totalExcerptCell = totalExcerptsRow.createCell(0);
    totalExcerptCell.setCellStyle(titleStyle);
    totalExcerptCell.setCellValue("Total Excerpts by Program");

    var style = createStyle("Arial", (short) 12, false, true, HorizontalAlignment.CENTER);

    var row1 = sheet.createRow(++startRowIndex);
    var cellA = row1.createCell(0);
    cellA.setCellStyle(style);
    cellA.setCellValue("Talk Radio");
    var cellC = row1.createCell(2);
    cellC.setCellStyle(style);
    cellC.setCellValue("News Radio");
    var cellE = row1.createCell(4);
    cellE.setCellStyle(style);
    cellE.setCellValue("Television");

    var row2 = sheet.createRow(++startRowIndex);
    cellA = row2.createCell(0);
    cellA.setCellStyle(style);
    cellA.setCellValue("(Series)");
    var cellB = row2.createCell(1);
    cellB.setCellStyle(style);
    cellB.setCellValue("(#)");
    cellC = row2.createCell(2);
    cellC.setCellStyle(style);
    cellC.setCellValue("(Source)");
    var cellD = row2.createCell(3);
    cellD.setCellStyle(style);
    cellD.setCellValue("(#)");
    cellE = row2.createCell(4);
    cellE.setCellStyle(style);
    cellE.setCellValue("(Series)");
    var cellF = row2.createCell(5);
    cellF.setCellStyle(style);
    cellF.setCellValue("(#)");

    for (var i = 0; i < maxRows; i++) {

      var row = sheet.createRow(++startRowIndex);

      if (i < talkRadioSeries.size()) {
        var talkRadioKey = talkRadioKeys.get(i);
        var talkRadioRows = talkRadioSeries.get(talkRadioKey);

        var name = talkRadioKey.isPresent() ? talkRadioKey.get().getName() : "NOT SET";
        cellA = row.createCell(0);
        cellA.setCellStyle(style);
        cellA.setCellValue(name);

        cellB = row.createCell(1);
        cellB.setCellStyle(style);
        cellB.setCellValue(talkRadioRows.size());
      }

      if (i < newsRadioSources.size()) {
        var newsRadioKey = newsRadioKeys.get(i);
        var newsRadioRows = newsRadioSources.get(newsRadioKey);

        var name = newsRadioKey != null ? newsRadioKey : "NOT SET";
        cellA = row.createCell(2);
        cellA.setCellStyle(style);
        cellA.setCellValue(name);

        cellB = row.createCell(3);
        cellB.setCellStyle(style);
        cellB.setCellValue(newsRadioRows.size());
      }

      if (i < televisionSeries.size()) {
        var televisionKey = televisionKeys.get(i);
        var televisionRows = televisionSeries.get(televisionKey);

        var name = televisionKey.isPresent() ? televisionKey.get().getName() : "NOT SET";
        cellA = row.createCell(4);
        cellA.setCellStyle(style);
        cellA.setCellValue(name);

        cellB = row.createCell(5);
        cellB.setCellStyle(style);
        cellB.setCellValue(televisionRows.size());
      }

    }

    return startRowIndex;
  }

  private int addTotalRunningTimeByProgram(int startRowIndex) {
    var titleStyle = createStyle("Arial", (short) 16, true, false, HorizontalAlignment.CENTER);

    var totalExcerptsRow = sheet.createRow(startRowIndex);
    sheet.addMergedRegion(CellRangeAddress.valueOf(String.format("A%s:E%<s", startRowIndex + 1)));
    var totalExcerptCell = totalExcerptsRow.createCell(0);
    totalExcerptCell.setCellStyle(titleStyle);
    totalExcerptCell.setCellValue("Total Running Time by Program");

    var style = createStyle("Arial", (short) 12, false, true, HorizontalAlignment.CENTER);

    var row1 = sheet.createRow(++startRowIndex);
    var cellA = row1.createCell(0);
    cellA.setCellStyle(style);
    cellA.setCellValue("Talk Radio");
    var cellC = row1.createCell(2);
    cellC.setCellStyle(style);
    cellC.setCellValue("News Radio");
    var cellE = row1.createCell(4);
    cellE.setCellStyle(style);
    cellE.setCellValue("Television");

    var row2 = sheet.createRow(++startRowIndex);
    cellA = row2.createCell(0);
    cellA.setCellStyle(style);
    cellA.setCellValue("(Series)");
    var cellB = row2.createCell(1);
    cellB.setCellStyle(style);
    cellB.setCellValue("(#)");
    cellC = row2.createCell(2);
    cellC.setCellStyle(style);
    cellC.setCellValue("(Source)");
    var cellD = row2.createCell(3);
    cellD.setCellStyle(style);
    cellD.setCellValue("(#)");
    cellE = row2.createCell(4);
    cellE.setCellStyle(style);
    cellE.setCellValue("(Series)");
    var cellF = row2.createCell(5);
    cellF.setCellStyle(style);
    cellF.setCellValue("(#)");

    // Iterate through groups.

    for (var i = 0; i < maxRows; i++) {

      var row = sheet.createRow(++startRowIndex);

      if (i < talkRadioSeries.size()) {
        var talkRadioKey = talkRadioKeys.get(i);
        var talkRadioRows = talkRadioSeries.get(talkRadioKey);

        var name = talkRadioKey.isPresent() ? talkRadioKey.get().getName() : "NOT SET";
        cellA = row.createCell(0);
        cellA.setCellStyle(style);
        cellA.setCellValue(name);

        var total = talkRadioRows.stream().filter(c -> c.getFileReferences().size() > 0)
            .mapToInt(c -> c.getFileReferences().get(0).getRunningTime()).sum();
        var time = calculateRunningTime(total);
        cellB = row.createCell(1);
        cellB.setCellStyle(style);
        cellB.setCellValue(time);
      }

      if (i < newsRadioSources.size()) {
        var newsRadioKey = newsRadioKeys.get(i);
        var newsRadioRows = newsRadioSources.get(newsRadioKey);

        var name = newsRadioKey != null ? newsRadioKey : "NOT SET";
        cellA = row.createCell(2);
        cellA.setCellStyle(style);
        cellA.setCellValue(name);

        var total = newsRadioRows.stream().filter(c -> c.getFileReferences().size() > 0)
            .mapToInt(c -> c.getFileReferences().get(0).getRunningTime()).sum();
        var time = calculateRunningTime(total);
        cellB = row.createCell(3);
        cellB.setCellStyle(style);
        cellB.setCellValue(time);
      }

      if (i < televisionSeries.size()) {
        var televisionKey = televisionKeys.get(i);
        var televisionRows = televisionSeries.get(televisionKey);

        var name = televisionKey.isPresent() ? televisionKey.get().getName() : "NOT SET";
        cellA = row.createCell(4);
        cellA.setCellStyle(style);
        cellA.setCellValue(name);

        var total = televisionRows.stream().filter(c -> c.getFileReferences().size() > 0)
            .mapToInt(c -> c.getFileReferences().get(0).getRunningTime()).sum();
        var time = calculateRunningTime(total);
        cellB = row.createCell(5);
        cellB.setCellStyle(style);
        cellB.setCellValue(time);
      }

    }

    return startRowIndex;
  }

  private int addPercentOfTotalRunningTimeByProgram(int startRowIndex) {
    var titleStyle = createStyle("Arial", (short) 16, true, false, HorizontalAlignment.CENTER);

    var totalExcerptsRow = sheet.createRow(startRowIndex);
    sheet.addMergedRegion(CellRangeAddress.valueOf(String.format("A%s:E%<s", startRowIndex + 1)));
    var totalExcerptCell = totalExcerptsRow.createCell(0);
    totalExcerptCell.setCellStyle(titleStyle);
    totalExcerptCell.setCellValue("% of Total Running Time by Program");

    var style = createStyle("Arial", (short) 12, false, true, HorizontalAlignment.CENTER);

    var row1 = sheet.createRow(++startRowIndex);
    var cellA = row1.createCell(0);
    cellA.setCellStyle(style);
    cellA.setCellValue("Talk Radio");
    var cellC = row1.createCell(2);
    cellC.setCellStyle(style);
    cellC.setCellValue("News Radio");
    var cellE = row1.createCell(4);
    cellE.setCellStyle(style);
    cellE.setCellValue("Television");

    var row2 = sheet.createRow(++startRowIndex);
    cellA = row2.createCell(0);
    cellA.setCellStyle(style);
    cellA.setCellValue("(Series)");
    var cellB = row2.createCell(1);
    cellB.setCellStyle(style);
    cellB.setCellValue("(%)");
    cellC = row2.createCell(2);
    cellC.setCellStyle(style);
    cellC.setCellValue("(Source)");
    var cellD = row2.createCell(3);
    cellD.setCellStyle(style);
    cellD.setCellValue("(%)");
    cellE = row2.createCell(4);
    cellE.setCellStyle(style);
    cellE.setCellValue("(Series)");
    var cellF = row2.createCell(5);
    cellF.setCellStyle(style);
    cellF.setCellValue("(%)");

    var totalTalkRadio = talkRadio.stream().filter((c) -> c.getFileReferences().size() > 0)
        .mapToInt((c) -> c.getFileReferences().get(0).getRunningTime()).sum();
    var totalNewsRadio = newsRadio.stream().filter((c) -> c.getFileReferences().size() > 0)
        .mapToInt((c) -> c.getFileReferences().get(0).getRunningTime()).sum();
    var totalTelevision = television.stream().filter((c) -> c.getFileReferences().size() > 0)
        .mapToInt((c) -> c.getFileReferences().get(0).getRunningTime()).sum();

    for (var i = 0; i < maxRows; i++) {
      var row = sheet.createRow(++startRowIndex);

      if (i < talkRadioSeries.size()) {
        var talkRadioKey = talkRadioKeys.get(i);
        var talkRadioRows = talkRadioSeries.get(talkRadioKey);

        var name = talkRadioKey.isPresent() ? talkRadioKey.get().getName() : "NOT SET";
        cellA = row.createCell(0);
        cellA.setCellStyle(style);
        cellA.setCellValue(name);

        var totalSeries = talkRadioRows.stream().filter(c -> c.getFileReferences().size() > 0)
            .mapToInt(c -> c.getFileReferences().get(0).getRunningTime()).sum();
        var time = calculateRunningTimePercent(totalSeries, totalTalkRadio);
        cellB = row.createCell(1);
        cellB.setCellStyle(style);
        cellB.setCellValue(time);
      }

      if (i < newsRadioSources.size()) {
        var newsRadioKey = newsRadioKeys.get(i);
        var newsRadioRows = newsRadioSources.get(newsRadioKey);

        var name = newsRadioKey != null ? newsRadioKey : "NOT SET";
        cellA = row.createCell(2);
        cellA.setCellStyle(style);
        cellA.setCellValue(name);

        var totalSource = newsRadioRows.stream().filter(c -> c.getFileReferences().size() > 0)
            .mapToInt(c -> c.getFileReferences().get(0).getRunningTime()).sum();
        var time = calculateRunningTimePercent(totalSource, totalNewsRadio);
        cellB = row.createCell(3);
        cellB.setCellStyle(style);
        cellB.setCellValue(time);
      }

      if (i < televisionSeries.size()) {
        var televisionKey = televisionKeys.get(i);
        var televisionRows = televisionSeries.get(televisionKey);

        var name = televisionKey.isPresent() ? televisionKey.get().getName() : "NOT SET";
        cellA = row.createCell(4);
        cellA.setCellStyle(style);
        cellA.setCellValue(name);

        var totalSeries = televisionRows.stream().filter(c -> c.getFileReferences().size() > 0)
            .mapToInt(c -> c.getFileReferences().get(0).getRunningTime()).sum();
        var time = calculateRunningTimePercent(totalSeries, totalTelevision);
        cellB = row.createCell(5);
        cellB.setCellStyle(style);
        cellB.setCellValue(time);
      }

    }

    return startRowIndex;
  }

  private int addPercentOfTotalRunningTime(int startRowIndex) {
    var titleStyle = createStyle("Arial", (short) 16, true, false, HorizontalAlignment.CENTER);

    var totalExcerptsRow = sheet.createRow(startRowIndex);
    sheet.addMergedRegion(CellRangeAddress.valueOf(String.format("A%s:E%<s", startRowIndex + 1)));
    var totalExcerptCell = totalExcerptsRow.createCell(0);
    totalExcerptCell.setCellStyle(titleStyle);
    totalExcerptCell.setCellValue("% of Total Running Time");

    var style = createStyle("Arial", (short) 12, false, true, HorizontalAlignment.CENTER);

    var row1 = sheet.createRow(++startRowIndex);
    var cellA = row1.createCell(1);
    cellA.setCellStyle(style);
    cellA.setCellValue("(Source)");
    var cellB = row1.createCell(2);
    cellB.setCellStyle(style);
    cellB.setCellValue("(%)");

    var totalRunningTime = Arrays.stream(content).filter((c) -> c.getFileReferences().size() > 0)
        .mapToInt((c) -> c.getFileReferences().get(0).getRunningTime()).sum();
    var sources = newsRadio.stream().collect(Collectors.groupingBy(Content::getSource));
    var sourceKeys = new ArrayList<String>(sources.keySet());

    for (var i = 0; i < maxRows; i++) {
      var row = sheet.createRow(++startRowIndex);

      if (i < sources.size()) {
        var sourceKey = sourceKeys.get(i);
        var sourceRows = sources.get(sourceKey);

        var name = sourceKey != null ? sourceKey : "NOT SET";
        cellA = row.createCell(0);
        cellA.setCellStyle(style);
        cellA.setCellValue(name);

        var totalSource = sourceRows.stream().filter(c -> c.getFileReferences().size() > 0)
            .mapToInt(c -> c.getFileReferences().get(0).getRunningTime()).sum();
        var time = calculateRunningTimePercent(totalSource, totalRunningTime);
        cellB = row.createCell(1);
        cellB.setCellStyle(style);
        cellB.setCellValue(time);
      }
    }

    return startRowIndex;
  }

  private int addTotals(int startRowIndex) {
    var style = createStyle("Arial", (short) 12, true, true, HorizontalAlignment.LEFT);

    // Total number of Excerpts
    var row1 = sheet.createRow(startRowIndex);
    sheet.addMergedRegion(CellRangeAddress.valueOf(String.format("A%s:C%<s", startRowIndex + 1)));
    var cellA = row1.createCell(0);
    cellA.setCellStyle(style);
    cellA.setCellValue("Total number of Excerpts");

    var cellB = row1.createCell(3);
    cellB.setCellStyle(style);
    cellB.setCellValue("(#)");

    var cellC = row1.createCell(4);
    cellC.setCellStyle(style);
    cellC.setCellValue(content.length);

    // Total number of Excerpts which do not meet the definition of Qualified
    // Subject Matter
    row1 = sheet.createRow(startRowIndex += 2);
    sheet.addMergedRegion(CellRangeAddress.valueOf(String.format("A%s:C%<s", startRowIndex + 1)));
    cellA = row1.createCell(0);
    cellA.setCellStyle(style);
    cellA.setCellValue("Total Number of Excerpts which do not meet the definition of Qualified Subject Matter");

    cellB = row1.createCell(3);
    cellB.setCellStyle(style);
    cellB.setCellValue("(#)");

    var action = actionService.findByName("Non Qualified Subject");
    var actionId = action.isPresent() ? action.get().getId() : 0;
    var totalNonQualified = Arrays.stream(content)
        .filter((c) -> c.getContentActions().size() > 0
            && c.getContentActions().stream().filter((a) -> a.getActionId() == actionId)
                .count() == 1)
        .count();
    cellC = row1.createCell(4);
    cellC.setCellStyle(style);
    cellC.setCellValue(totalNonQualified);

    // Total number of Excerpts over 10 minutes
    row1 = sheet.createRow(startRowIndex += 2);
    sheet.addMergedRegion(CellRangeAddress.valueOf(String.format("A%s:C%<s", startRowIndex + 1)));
    cellA = row1.createCell(0);
    cellA.setCellStyle(style);
    cellA.setCellValue("Total number of Excerpts over 10 minutes");

    cellB = row1.createCell(3);
    cellB.setCellStyle(style);
    cellB.setCellValue("(#)");

    var tenMinutes = 1000 * 60 * 10;
    var totalOver10Minutes = Arrays.stream(content)
        .filter((c) -> c.getFileReferences().size() > 0
            && c.getFileReferences().stream().filter((f) -> f.getRunningTime() > tenMinutes)
                .count() >= 1)
        .count();
    cellC = row1.createCell(4);
    cellC.setCellStyle(style);
    cellC.setCellValue(totalOver10Minutes);

    // Percentage over 10 minutes
    row1 = sheet.createRow(startRowIndex += 2);
    sheet.addMergedRegion(CellRangeAddress.valueOf(String.format("A%s:C%<s", startRowIndex + 1)));
    cellA = row1.createCell(0);
    cellA.setCellStyle(style);
    cellA.setCellValue("Percentage over 10 minutes");

    cellB = row1.createCell(3);
    cellB.setCellStyle(style);
    cellB.setCellValue("(%)");

    var percentage = totalOver10Minutes > 0 ? totalOver10Minutes / content.length : 0;
    cellC = row1.createCell(4);
    cellC.setCellStyle(style);
    cellC.setCellValue(percentage);

    return startRowIndex;
  }

  private int addStaffSummary(int startRowIndex, ZonedDateTime from, ZonedDateTime to) {
    var titleStyle = createStyle("Arial", (short) 16, true, false, HorizontalAlignment.LEFT);

    var titleRow = sheet.createRow(startRowIndex);
    sheet.addMergedRegion(CellRangeAddress.valueOf(String.format("A%s:E%<s", startRowIndex + 1)));
    var titleCell = titleRow.createCell(0);
    titleCell.setCellStyle(titleStyle);
    titleCell.setCellValue("Staff Summary");

    var headingStyle = createStyle("Arial", (short) 12, true, false, HorizontalAlignment.CENTER);

    var row1 = sheet.createRow(++startRowIndex);
    var cellA = row1.createCell(0);
    cellA.setCellStyle(headingStyle);
    cellA.setCellValue("Staff");
    var cellB = row1.createCell(1);
    cellB.setCellStyle(headingStyle);
    cellB.setCellValue("CBRA Hours");

    var style = createStyle("Arial", (short) 12, false, true, HorizontalAlignment.CENTER);

    var row2 = sheet.createRow(++startRowIndex);
    cellA = row2.createCell(0);
    cellA.setCellStyle(style);
    cellA.setCellValue("[Username]");
    cellB = row2.createCell(1);
    cellB.setCellStyle(style);
    cellB.setCellValue("[hours]");

    var timeTracking = userService.getTimeTracking(from, to);
    var timeKeys = new ArrayList<User>(timeTracking.keySet());

    for (var i = 0; i < maxRows; i++) {
      var row = sheet.createRow(++startRowIndex);

      if (i < timeTracking.size()) {
        var userKey = timeKeys.get(i);
        var userRows = timeTracking.get(userKey);

        var name = userKey != null ? userKey.getUsername() : "NOT SET";
        cellA = row.createCell(0);
        cellA.setCellStyle(style);
        cellA.setCellValue(name);

        var totalEffort = userRows.stream().mapToDouble(c -> c.getEffort()).sum();
        cellB = row.createCell(1);
        cellB.setCellStyle(style);
        cellB.setCellValue(totalEffort);
      }
    }

    return startRowIndex;
  }

  private int addDatabaseEntries(int startRowIndex) {
    var titleStyle = createStyle("Arial", (short) 16, true, false, HorizontalAlignment.LEFT);

    var titleRow = sheet.createRow(startRowIndex);
    sheet.addMergedRegion(CellRangeAddress.valueOf(String.format("A%s:E%<s", startRowIndex + 1)));
    var titleCell = titleRow.createCell(0);
    titleCell.setCellStyle(titleStyle);
    titleCell.setCellValue("Database Entries");

    var headingStyle = createStyle("Arial", (short) 12, true, false, HorizontalAlignment.CENTER);

    var row1 = sheet.createRow(++startRowIndex);
    var cellA = row1.createCell(0);
    cellA.setCellStyle(headingStyle);
    cellA.setCellValue("Percentage of CBRA Database Entries");

    var style = createStyle("Arial", (short) 12, false, true, HorizontalAlignment.CENTER);

    var row2 = sheet.createRow(++startRowIndex);
    cellA = row2.createCell(0);
    cellA.setCellStyle(style);
    cellA.setCellValue("(%)");

    var total = talkRadioSeries.size() + newsRadioSources.size() + televisionSeries.size();
    var percentage = total <= 0 ? 0 : total / content.length;
    var row3 = sheet.createRow(++startRowIndex);
    cellA = row3.createCell(0);
    cellA.setCellStyle(style);
    cellA.setCellValue(percentage);

    return startRowIndex;
  }

  private float calculateRunningTimePercent(int runningTime, int totalRunningTime) {
    if (runningTime <= 0)
      return runningTime;

    return calculateRunningTime(runningTime) / calculateRunningTime(totalRunningTime);
  }

  private float calculateRunningTime(int runningTime) {
    return toFloat(toDuration(runningTime));
  }

  private Duration toDuration(int milliseconds) {
    return Duration.ofMillis(milliseconds);
  }

  private float toFloat(Duration duration) {
    if (duration == null)
      return 0.0f;
    var totalMinutes = duration.toMinutes();
    if (totalMinutes <= 0)
      return 0f;
    var hours = totalMinutes / 60;
    var minutes = totalMinutes % 60;
    return hours + ((float) minutes / 60);
  }
}
