package ca.bc.gov.tno.dal.db;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.dal.db.services.interfaces.ILicenseService;

@Component
public class LicenseTest {
  ILicenseService licenseService;

  @Autowired
  public LicenseTest(final ILicenseService licenseService) {
    this.licenseService = licenseService;
  }

  public void Run() {
    FindAll();
  }

  public void FindAll() {
    System.out.println("Fetching licenses");
    var licenses = licenseService.findAll();
    licenses.forEach(l -> System.out.println(l.getName()));
  }
}
