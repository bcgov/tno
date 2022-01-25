package ca.bc.gov.tno.areas.admin.controllers;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.ILicenseService;
import ca.bc.gov.tno.dal.db.entities.License;

/**
 * Endpoints to communicate with the TNO DB licenses.
 */
@RolesAllowed("administrator")
@RestController("AdminLicenseController")
@RequestMapping("/admin/licenses")
public class LicenseController {

  @Autowired
  private ILicenseService licenseService;

  /**
   * Request a list of all licenses from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<License> findAll() {
    var licenses = licenseService.findAll();
    return licenses;
  }

  /**
   * Request a list of all licenses from the db.
   *
   * @param id The primary key.
   * @return
   */
  @GetMapping(path = "/{id}", produces = MediaType.APPLICATION_JSON_VALUE)
  public License findById(@PathVariable(required = true) Integer id) {
    var license = licenseService.findById(id).orElse(null);
    return license;
  }

  /**
   * Add a new license to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = "/", consumes = MediaType.APPLICATION_JSON_VALUE, produces = MediaType.APPLICATION_JSON_VALUE)
  public License add(@RequestBody License model) {
    var license = licenseService.add(model);
    return license;
  }

  /**
   * Update the license in the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @PutMapping(path = "/{id}", consumes = MediaType.APPLICATION_JSON_VALUE, produces = MediaType.APPLICATION_JSON_VALUE)
  public License update(@PathVariable Integer id, @RequestBody License model) {
    var license = licenseService.add(model);
    return license;
  }

  /**
   * Delete the license from the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{id}", consumes = MediaType.APPLICATION_JSON_VALUE, produces = MediaType.APPLICATION_JSON_VALUE)
  public License delete(@PathVariable Integer id, @RequestBody License model) {
    licenseService.delete(model);
    return model;
  }

}
