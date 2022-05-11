package ca.bc.gov.tno.services.nlp;

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

import ca.bc.gov.tno.services.events.ServiceStartEvent;

/**
 * NlpServiceApp class, provides a process that pulls messages from Kafka
 * topics, performs Natural Language Processes, and pushes the content back into
 * Kafka. topics.
 */
@EnableJpaRepositories(basePackages = { "ca.bc.gov.tno.dal.db" })
@EntityScan(basePackages = { "ca.bc.gov.tno.dal.db" })
@SpringBootApplication(scanBasePackages = { "ca.bc.gov.tno.services.nlp", "ca.bc.gov.tno.dal.db",
    "ca.bc.gov.tno.services" })
public class NlpServiceApp implements ApplicationRunner {
  private static final Logger logger = LogManager.getLogger(NlpServiceApp.class);

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
      var app = new SpringApplication(NlpServiceApp.class);
      app.setWebApplicationType(WebApplicationType.SERVLET);
      app.run(args);
    } catch (Exception ex) {
      logger.fatal(ex);
      System.exit(1);
    }
  }

  /**
   * Run the Kafka consumer process.
   */
  @Override
  public void run(ApplicationArguments args) throws Exception {
    logger.info("NLP Kafka Consumer and Producer - Started");
    var event = new ServiceStartEvent(this);
    applicationEventPublisher.publishEvent(event);
  }
}
