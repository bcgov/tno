package ca.bc.gov.tno.services.kafka.events;

import org.apache.kafka.clients.consumer.ConsumerRecord;
import org.springframework.context.ApplicationEvent;
import org.springframework.scheduling.annotation.Async;

/**
 * ConsumerRecordReceivedEvent class, provides an event when a record has been
 * received successfully from Kafka. It is now ready to be sent to
 * Elasticsearch.
 */
@Async
public class ConsumerRecordReceivedEvent<K, R> extends ApplicationEvent {
  /**
   * The consumer record pulled from Kafka.
   */
  private final ConsumerRecord<K, R> record;

  /**
   * Creates a new instance of an ConsumerRecordReceivedEvent, initializes with
   * specified parameters.
   * 
   * @param source The event originator.
   * @param record The Kafka consumer record.
   */
  public ConsumerRecordReceivedEvent(final Object source, final ConsumerRecord<K, R> record) {
    super(source);
    this.record = record;
  }

  /**
   * Get the Kafka consumer record.
   * 
   * @return The consumer record from Kafka.
   */
  public ConsumerRecord<K, R> getRecord() {
    return record;
  }
}
