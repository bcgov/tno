package ca.bc.gov.tno.nlp;

import org.springframework.boot.CommandLineRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.ComponentScan;
import org.springframework.stereotype.Component;

/**
 * Use this CommandLineRunner application to test the database DAL.
 */
@Component
@ComponentScan
@SpringBootApplication(scanBasePackages = { "ca.bc.gov.tno.dal.db.*" })
public class App implements CommandLineRunner {
  public static void main(String[] args) {
    System.out.println("TNO Database DAL Console Application");

    SpringApplication.run(App.class, args);
    System.exit(0);
  }

  @Override
  public void run(String... args) throws Exception {
  }
}
