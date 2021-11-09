package ca.bc.gov.tno.nlp.events;

import org.apache.kafka.clients.consumer.ConsumerRecord;
import org.springframework.context.ApplicationEvent;
import org.springframework.scheduling.annotation.Async;

import ca.bc.gov.tno.nlp.models.SourceContent;

/**
 * ConsumerRecordReceivedEvent class, provides an event when a record has been
 * received successfully from Kafka. It is now ready to be processed.
 */
@Async
public class ConsumerRecordReceivedEvent extends ApplicationEvent {
  private ConsumerRecord<String, SourceContent> record;

  /**
   * Creates a new instance of an ConsumerRecordReceivedEvent, initializes with
   * specified parameters.
   * 
   * @param source
   * @param record
   */
  public ConsumerRecordReceivedEvent(Object source, ConsumerRecord<String, SourceContent> record) {
    super(source);
    this.record = record;
  }

  public ConsumerRecord<String, SourceContent> getRecord() {
    return record;
  }
}
