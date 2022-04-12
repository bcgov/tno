package ca.bc.gov.tno.areas.editor.controllers;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.ILicenseService;
import ca.bc.gov.tno.areas.editor.models.LicenseModel;

/**
 * Endpoints to communicate with the TNO DB licenses.
 */
@RolesAllowed({ "administrator", "editor" })
@RestController("EditorLicenseController")
@RequestMapping({ "/editor/licenses", "/api/editor/licenses" })
public class LicenseController {

  /**
   * DAL for license.
   */
  @Autowired
  private ILicenseService licenseService;

  /**
   * Request a list of all licenses from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<LicenseModel> findAll() {
    var results = licenseService.findAll();
    var models = results.stream().map(c -> new LicenseModel(c)).toList();
    return models;
  }

}
