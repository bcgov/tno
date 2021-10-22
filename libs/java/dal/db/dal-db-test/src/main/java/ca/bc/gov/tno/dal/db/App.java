package ca.bc.gov.tno.dal.db;

import org.hibernate.Hibernate;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.CommandLineRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceService;
import ca.bc.gov.tno.dal.db.services.interfaces.IUserService;
import ca.bc.gov.tno.dal.db.services.interfaces.ILicenseService;

/**
 * Use this CommandLineRunner application to test the database DAL.
 */
@Component
@SpringBootApplication(scanBasePackages = { "ca.bc.gov.tno.dal.db.*" })
public class App implements CommandLineRunner {

  @Autowired
  IDataSourceService dataSourceService;

  @Autowired
  IUserService userService;

  @Autowired
  ILicenseService licenseService;

  public static void main(String[] args) {
    System.out.println("TNO Database DAL Console Application");
    SpringApplication.run(App.class, args);
  }

  @Override
  public void run(String... args) throws Exception {
    System.out.println("Fetching users");
    var users = userService.findAll();
    users.forEach(u -> System.out.println(u.getUsername()));

    System.out.println("Fetching data sources");
    var sources = dataSourceService.findAll();
    sources.forEach(ds -> System.out.println(ds.getName() + " " + ds.getConnection()));

    System.out.println("Fetching licenses");
    var licenses = licenseService.findAll();
    licenses.forEach(l -> System.out.println(l.getName()));
  }
}
