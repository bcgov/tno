package ca.bc.gov.tno.services.nlp.parsers;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.jsoup.Jsoup;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;
import org.springframework.context.annotation.Scope;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.models.NlpContent;
import ca.bc.gov.tno.services.nlp.events.ConsumerRecordReceivedEvent;
import ca.bc.gov.tno.services.nlp.events.ContentParsedEvent;
import ca.bc.gov.tno.services.nlp.events.ErrorEvent;

/**
 * ContentParser class, provides a way to parse through the content, clean it up
 * and prepare it for NLP.
 */
@Async
@Component
@Scope("prototype")
public class ContentParser implements ApplicationListener<ConsumerRecordReceivedEvent> {
  private static final Logger logger = LogManager.getLogger(ContentParser.class);

  @Autowired
  private ApplicationEventPublisher applicationEventPublisher;

  /**
   * Remove any HTML from the content body. Then fire the ready event.
   */
  @Override
  public void onApplicationEvent(ConsumerRecordReceivedEvent event) {
    try {
      var record = event.getRecord();
      var content = record.value();

      logger.debug(String.format("Content parsing: '%s'", content.getUid()));

      var summary = Jsoup.parse(content.getSummary()).text();
      content.setSummary(summary);
      var body = Jsoup.parse(content.getBody()).text();
      content.setBody(body);

      var result = new NlpContent(content, record.topic(), record.partition(), record.offset());

      logger.info(String.format("Content has been parsed: '%s'", result.getUid()));

      var parsedEvent = new ContentParsedEvent(this, content, result);
      applicationEventPublisher.publishEvent(parsedEvent);
    } catch (Exception ex) {
      // TODO: Failed content needs to identified so that it can be rerun. Or it needs
      // to be pushed as it is into the next queue.
      var errorEvent = new ErrorEvent(this, ex);
      applicationEventPublisher.publishEvent(errorEvent);
    }
  }
}
