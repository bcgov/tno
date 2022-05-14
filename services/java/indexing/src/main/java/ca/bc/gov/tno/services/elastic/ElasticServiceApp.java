package ca.bc.gov.tno.services.elastic;

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
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.services.events.ServiceStartEvent;

/**
 * ElasticServiceApp class, provides a process that pulls messages from Kafka
 * topics and inserts them into Elasticsearch for indexing.
 */
@Service
@EnableJpaRepositories(basePackages = { "ca.bc.gov.tno.dal.db" })
@EntityScan(basePackages = { "ca.bc.gov.tno.dal.db" })
@SpringBootApplication(scanBasePackages = { "ca.bc.gov.tno.services.elastic", "ca.bc.gov.tno.dal.db",
    "ca.bc.gov.tno.dal.elastic", "ca.bc.gov.tno.services" })
public class ElasticServiceApp implements ApplicationRunner {
  private static final Logger logger = LogManager.getLogger(ElasticServiceApp.class);

  private final ApplicationEventPublisher eventPublisher;

  /**
   * Creates a new instance of an ElasticServiceApp object, initializes with
   * specified parameters.
   * 
   * @param eventPublisher Application event publisher.
   */
  @Autowired
  public ElasticServiceApp(final ApplicationEventPublisher eventPublisher) {
    this.eventPublisher = eventPublisher;
  }

  /**
   * Commandline application start point.
   * 
   * @param args
   */
  public static void main(String[] args) {
    try {
      System.setProperty("pid", ProcessIdUtil.getProcessId());
      var app = new SpringApplication(ElasticServiceApp.class);
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
    logger.info("Elasticsearch Kafka Consumer Started");
    var event = new ServiceStartEvent(this);
    eventPublisher.publishEvent(event);
  }
}
