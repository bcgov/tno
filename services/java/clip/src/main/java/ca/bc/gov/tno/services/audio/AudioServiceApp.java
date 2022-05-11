package ca.bc.gov.tno.services.audio;

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
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.services.events.ServiceStartEvent;

/**
 * AudioServiceApp class, provides a process that fetches audio
 * feeds and imports them into a Kafka Topic.
 */
@Service
@EntityScan(basePackages = { "ca.bc.gov.tno.services" })
@SpringBootApplication(scanBasePackages = {
    "ca.bc.gov.tno.services.audio",
    "ca.bc.gov.tno.services" })
public class AudioServiceApp implements ApplicationRunner {
  private static final Logger logger = LogManager.getLogger(AudioServiceApp.class);

  private final ApplicationEventPublisher eventPublisher;

  /**
   * Creates a new instance of a AudioServiceApp object, initializes with
   * specified parameters.
   * 
   * @param eventPublisher Application event publisher.
   */
  @Autowired
  public AudioServiceApp(final ApplicationEventPublisher eventPublisher) {
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
      var app = new SpringApplication(AudioServiceApp.class);
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
    logger.info("Kafka Audio Producer Started");
    var event = new ServiceStartEvent(this);
    eventPublisher.publishEvent(event);
  }

}
