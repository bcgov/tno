package ca.bc.gov.tno.services.kafka;

import java.util.Arrays;
import java.util.List;
import java.time.Duration;

import org.apache.kafka.clients.consumer.InvalidOffsetException;
import org.apache.kafka.clients.consumer.KafkaConsumer;
import org.apache.kafka.common.errors.InterruptException;
import org.apache.kafka.common.errors.WakeupException;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;
import org.springframework.scheduling.annotation.Async;

import ca.bc.gov.tno.services.ServiceState;
import ca.bc.gov.tno.services.ServiceStatus;
import ca.bc.gov.tno.services.kafka.config.KafkaConsumerConfig;
import ca.bc.gov.tno.services.kafka.events.ConsumerRecordReceivedEvent;
import ca.bc.gov.tno.services.events.ErrorEvent;
import ca.bc.gov.tno.services.events.ServiceStartEvent;

/**
 * BaseKafkaConsumerService class, provides a Kafka Consumer service that
 * continues to poll Kafka Topics for new messages. Publishes event each time it
 * pulls a new message.
 */
@Async
public abstract class BaseKafkaConsumerService<K, R> implements ApplicationListener<ServiceStartEvent> {
  private static final Logger logger = LogManager.getLogger(BaseKafkaConsumerService.class);

  /**
   * The service state.
   */
  protected final ServiceState state;

  /**
   * Kafka consumer configuration settings.
   */
  protected final KafkaConsumerConfig config;

  /**
   * The application event publisher.
   */
  protected final ApplicationEventPublisher eventPublisher;

  /**
   * A list of Kafka topics.
   */
  protected final List<String> topics;

  /**
   * The timeout duration.
   */
  protected final Duration timeout;

  /**
   * The kafka consumer.
   */
  protected final KafkaConsumer<K, R> consumer;

  /**
   * Create a new instance of a KafkaNlpConsumer object, initializes with
   * specified parameters.
   *
   * @param state          Service state.
   * @param config         Kafka configuration.
   * @param eventPublisher Application event publisher.
   */
  public BaseKafkaConsumerService(final ServiceState state, final KafkaConsumerConfig config,
      final ApplicationEventPublisher eventPublisher) {
    this.state = state;
    this.config = config;
    this.eventPublisher = eventPublisher;
    this.topics = Arrays.asList(config.getConsumerTopics().trim().split("\\s*,\\s*"));
    this.timeout = Duration.ofMillis(config.getPollTimeout());
    this.consumer = initConsumer();
  }

  /**
   * Initialize the Kafka consumer. This is called in the constructor.
   */
  protected abstract KafkaConsumer<K, R> initConsumer();

  /**
   * Run the Kafka Consumer service and continue polling Kafka for new messages.
   * Provides a way to pause and resume the service.
   */
  @Override
  public void onApplicationEvent(ServiceStartEvent event) {
    try {
      logger.info("Consumer started.");
      state.setStatus(ServiceStatus.running);
      consumer.subscribe(topics);
      var assignment = consumer.assignment();
      var topic = String.join(", ", topics);

      while (state.getStatus() != ServiceStatus.sleeping) {

        if (state.getStatus() == ServiceStatus.pause) {
          consumer.pause(assignment);
          state.setStatus(ServiceStatus.paused);
        } else if (state.getStatus() == ServiceStatus.resume) {
          consumer.resume(assignment);
          state.setStatus(ServiceStatus.running);
        }

        logger.debug(String.format("Polling: (%s) topic: %s", state.getStatus(), topic));
        var records = consumer.poll(timeout);

        // Successfully polled Kafka, reset failures.
        state.setFailedAttempts(0);

        for (var record : records) {
          logger.info(String.format("Record received: key: %s", record.key()));
          // TODO: Need to record any failures so that they can be identified and rerun.
          var recordReceivedEvent = new ConsumerRecordReceivedEvent<K, R>(this, record);
          eventPublisher.publishEvent(recordReceivedEvent);
        }
      }
    } catch (InvalidOffsetException | WakeupException | InterruptException | ArithmeticException ex) {
      state.setStatus(ServiceStatus.sleeping);
      var errorEvent = new ErrorEvent(this, ex);
      eventPublisher.publishEvent(errorEvent);
    }
  }

  /**
   * Close the Kafka consumer.
   */
  @Override
  public void finalize() {
    consumer.close();
  }
}
