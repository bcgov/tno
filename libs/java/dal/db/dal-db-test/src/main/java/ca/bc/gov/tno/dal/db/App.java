package ca.bc.gov.tno.dal.db;

import java.util.UUID;

import org.hibernate.exception.ConstraintViolationException;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.CommandLineRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.WebApplicationType;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.ComponentScan;
import org.springframework.dao.DataIntegrityViolationException;
import org.springframework.orm.ObjectOptimisticLockingFailureException;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.dal.db.entities.ContentReference;
import ca.bc.gov.tno.dal.db.entities.ContentReferencePK;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentReferenceService;
import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceService;
import ca.bc.gov.tno.dal.db.services.interfaces.IUserService;
import ca.bc.gov.tno.dal.db.services.interfaces.ILicenseService;

/**
 * Use this CommandLineRunner application to test the database DAL.
 */
@Component
@ComponentScan
@SpringBootApplication(scanBasePackages = { "ca.bc.gov.tno.dal.db.*" })
public class App implements CommandLineRunner {

  @Autowired
  AppAuthenticationProvider authProvider;

  @Autowired
  IDataSourceService dataSourceService;

  @Autowired
  IUserService userService;

  @Autowired
  ILicenseService licenseService;

  @Autowired
  IContentReferenceService contentReferenceService;

  public static void main(String[] args) {
    System.out.println("TNO Database DAL Console Application");

    var app = new SpringApplication(App.class);
    app.setWebApplicationType(WebApplicationType.NONE);
    app.run(args);
    System.exit(0);
  }

  @Override
  public void run(String... args) throws Exception {

    var authReq = new UsernamePasswordAuthenticationToken("test", "password");
    var authentication = authProvider.authenticate(authReq);
    SecurityContextHolder.getContext().setAuthentication(authentication);

    TestUsers();
    TestDataSources();
    TestLicenses();
    TestContentReferences();
  }

  private void TestUsers() {
    System.out.println("Fetching users");
    var users = userService.findAll();
    users.forEach(u -> System.out.println(u.getUsername()));
  }

  private void TestDataSources() {
    System.out.println("Fetching data sources");
    var sources = dataSourceService.findAll();
    sources.forEach(ds -> System.out.println(ds.getName() + " " + ds.getConnection()));
  }

  private void TestLicenses() {
    System.out.println("Fetching licenses");
    var licenses = licenseService.findAll();
    licenses.forEach(l -> System.out.println(l.getName()));
  }

  private void TestContentReferences() {
    var reference = new ContentReference("NTLP", "uid-01", "topic");
    var added = contentReferenceService.add(reference);
    if (added.getCreatedOn() == null || added.getUpdatedOn() == null)
      throw new IllegalStateException("Audit dates were not set");
    if (added.getCreatedOn().compareTo(added.getUpdatedOn()) != 0)
      throw new IllegalStateException("Audit dates must be the same");

    added.setOffset(1);
    added.setCreatedBy("illegal");
    added.setCreatedById(UUID.randomUUID());
    var updated = contentReferenceService.update(added);
    if (added.getCreatedOn().compareTo(updated.getCreatedOn()) != 0)
      throw new IllegalStateException("Audit createdOn must not change");
    if (added.getCreatedById() != updated.getCreatedById())
      throw new IllegalStateException("Audit createdById must not change");
    if (added.getCreatedBy() != updated.getCreatedBy())
      throw new IllegalStateException("Audit createdBy must not change");
    if (added.getUpdatedOn().compareTo(updated.getUpdatedOn()) >= 0)
      throw new IllegalStateException("Audit updatedOn must be after prior timestamp");

    try {
      // Test for optimistic concurrency.
      contentReferenceService.update(reference);
    } catch (ObjectOptimisticLockingFailureException e) {
      System.out.println("Concurrency Failure: " + reference.getUpdatedOn());
    }

    try {
      // Test for duplication.
      var duplicate = new ContentReference("NTLP", "uid-01", "topic");
      contentReferenceService.add(duplicate);
    } catch (DataIntegrityViolationException e) {
      var cause = (ConstraintViolationException) e.getCause();
      if (!cause.getConstraintName().equals("pk_ContentReference"))
        throw new IllegalStateException("Constraint missing from table");
      System.out.println("Duplication Failure: " + reference.getSource());
    }

    contentReferenceService.delete(updated);
    var result = contentReferenceService.findById(new ContentReferencePK(reference.getSource(), reference.getUid()))
        .orElse(null);
    if (result != null)
      throw new IllegalStateException("Entity must be deleted");
  }
}
