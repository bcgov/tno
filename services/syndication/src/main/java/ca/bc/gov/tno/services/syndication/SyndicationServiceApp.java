package ca.bc.gov.tno.services.syndication;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.apache.logging.log4j.util.ProcessIdUtil;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.ApplicationArguments;
import org.springframework.boot.ApplicationRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.WebApplicationType;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.autoconfigure.domain.EntityScan;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.data.jpa.repository.config.EnableJpaRepositories;

import ca.bc.gov.tno.services.syndication.events.ServiceStartEvent;

/**
 * SyndicationServiceApp class, provides a process that fetches syndication
 * feeds and imports them into a Kafka Topic.
 */
@EnableJpaRepositories(basePackages = { "ca.bc.gov.tno.dal.db" })
@EntityScan(basePackages = { "ca.bc.gov.tno.dal.db" })
@SpringBootApplication(scanBasePackages = { "ca.bc.gov.tno.services.syndication", "ca.bc.gov.tno.dal.db" })
public class SyndicationServiceApp implements ApplicationRunner {
  private static final Logger logger = LogManager.getLogger(SyndicationServiceApp.class);

  @Autowired
  private ApplicationEventPublisher applicationEventPublisher;

  /**
   * Commandline application start point.
   * 
   * @param args
   */
  public static void main(String[] args) {
    try {
      System.setProperty("pid", ProcessIdUtil.getProcessId());
      var app = new SpringApplication(SyndicationServiceApp.class);
      app.setWebApplicationType(WebApplicationType.SERVLET);
      app.run(args);
    } catch (Exception ex) {
      logger.fatal(ex);
      System.exit(1);
    }
  }

  /**
   * Run the service.
   */
  @Override
  public void run(ApplicationArguments args) throws Exception {
    logger.info("Kafka Syndication Producer - Started");
    var event = new ServiceStartEvent(this);
    applicationEventPublisher.publishEvent(event);
  }

}
