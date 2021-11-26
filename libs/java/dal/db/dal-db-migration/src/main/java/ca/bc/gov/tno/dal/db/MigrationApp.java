package ca.bc.gov.tno.dal.db;

import org.springframework.boot.CommandLineRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.WebApplicationType;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.autoconfigure.domain.EntityScan;
import org.springframework.data.jpa.repository.config.EnableJpaRepositories;

@EnableJpaRepositories(basePackages = { "ca.bc.gov.tno.dal.db" })
@EntityScan(basePackages = { "ca.bc.gov.tno.dal.db" })
@SpringBootApplication(scanBasePackages = { "ca.bc.gov.tno.dal.db" })
public class MigrationApp implements CommandLineRunner {

  public static void main(String[] args) {
    var app = new SpringApplication(MigrationApp.class);
    app.setWebApplicationType(WebApplicationType.NONE);
    app.run(args);
    System.exit(0);
  }

  @Override
  public void run(String... args) throws Exception {
    System.out.println("TNO Database DAL Migration Tool");
  }
}
