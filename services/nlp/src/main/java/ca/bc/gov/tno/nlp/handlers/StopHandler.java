package ca.bc.gov.tno.nlp.handlers;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationListener;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.nlp.Global;
import ca.bc.gov.tno.nlp.NlpStatus;
import ca.bc.gov.tno.nlp.events.NlpStopEvent;

/**
 * StopHandler class, provides a way to handle errors and increment the failed
 * attempts. Once the failed attempts is equal to the maximum limit it will
 * inform the application to stop.
 */
@Component
public class StopHandler implements ApplicationListener<NlpStopEvent> {
  private static final Logger logger = LogManager.getLogger(StopHandler.class);

  @Autowired
  private Global global;

  /**
   * Log the error. Update the current failed attempts. Inform the application if
   * it should stop.
   */
  @Override
  public void onApplicationEvent(NlpStopEvent event) {
    logger.info("NLP stop requested");
    global.setStatus(NlpStatus.sleeping);
  }
}
