package ca.bc.gov.tno.dal.db;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.CommandLineRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.WebApplicationType;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.ComponentScan;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.stereotype.Component;

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
  UserTest userTest;

  @Autowired
  DataSourceTest dataSourceTest;

  @Autowired
  LicenseTest licenseTest;

  @Autowired
  ContentReferenceTest contentReferenceTest;

  @Autowired
  ContentTest contentTest;

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

    // TODO: Authenticate with Keycloak account and set username.
    // var principal = authentication.getPrincipal();
    // var roles = AdapterUtils.getRolesFromSecurityContext(principal);
    // var keycloakAccount = new SimpleKeycloakAccount(principal, roles, context)

    // userTest.Run();
    // dataSourceTest.Run();
    // licenseTest.Run();
    // contentReferenceTest.Run();
    contentTest.Run();
  }

}
