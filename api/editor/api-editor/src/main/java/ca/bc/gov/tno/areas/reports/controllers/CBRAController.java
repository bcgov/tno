package ca.bc.gov.tno.areas.reports.controllers;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.time.LocalDateTime;
import java.time.ZoneId;

import javax.annotation.security.RolesAllowed;
import javax.servlet.http.HttpServletResponse;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.core.io.InputStreamResource;
import org.springframework.core.io.Resource;
import org.springframework.format.annotation.DateTimeFormat;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.core.excel.CBRAReport;
import ca.bc.gov.tno.dal.db.services.interfaces.IActionService;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentService;
import ca.bc.gov.tno.dal.db.services.interfaces.IUserService;

/**
 * ActionController class, provides endpoints for actions.
 */
@RolesAllowed({ "administrator", "editor" })
@RestController("ReportsCBRAController")
@RequestMapping({ "/reports/cbra", "/api/reports/cbra" })
public class CBRAController {

  /**
   * DAL for action.
   */
  private final IContentService contentService;
  private final IUserService userService;
  private final IActionService actionService;

  /**
   * Creates a new instance of a ActionController object, initializes with
   * specified parameters.
   *
   * @param contentService Content service.
   * @param userService    User service.
   * @param actionService  Action service.
   */
  @Autowired
  public CBRAController(final IContentService contentService, final IUserService userService,
      final IActionService actionService) {
    this.contentService = contentService;
    this.userService = userService;
    this.actionService = actionService;
  }

  /**
   * Request a list of all actions from the db.
   *
   * @return
   * @throws IOException
   */
  @PostMapping(path = { "", "/" }, consumes = {
      MediaType.ALL_VALUE }, produces = MediaType.APPLICATION_OCTET_STREAM_VALUE)
  public ResponseEntity<Resource> generateReport(
      @RequestParam(required = true) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime from,
      @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime to,
      HttpServletResponse response) throws IOException {

    if (to == null)
      to = LocalDateTime.now();

    var report = new CBRAReport(contentService, userService, actionService);
    var workbook = report.generateReport(from.atZone(ZoneId.systemDefault()), to.atZone(ZoneId.systemDefault()));

    var outputStream = new ByteArrayOutputStream();
    workbook.write(outputStream);
    workbook.close();

    var inputStream = new ByteArrayInputStream(outputStream.toByteArray());
    var resource = new InputStreamResource(inputStream);

    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    var headers = new HttpHeaders();
    headers.setContentType(MediaType.parseMediaType(contentType));
    headers.setContentDispositionFormData("attachment", "cbra.xlsx");

    return ResponseEntity.ok()
        .headers(headers)
        .body(resource);
  }
}
