package ca.bc.gov.tno.services.nlp.kafka;

import java.util.Arrays;
import java.util.List;
import java.util.Properties;

import javax.annotation.PostConstruct;

import java.time.Duration;

import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.clients.consumer.ConsumerRecord;
import org.apache.kafka.clients.consumer.ConsumerRecords;
import org.apache.kafka.clients.consumer.InvalidOffsetException;
import org.apache.kafka.clients.consumer.KafkaConsumer;
import org.apache.kafka.common.errors.InterruptException;
import org.apache.kafka.common.errors.WakeupException;
import org.apache.kafka.common.serialization.StringDeserializer;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;
import org.springframework.context.event.EventListener;
import org.springframework.kafka.support.serializer.JsonDeserializer;
import org.springframework.kafka.support.serializer.JsonSerializer;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.models.SourceContent;
import ca.bc.gov.tno.models.Tag;
import ca.bc.gov.tno.services.nlp.Global;
import ca.bc.gov.tno.services.nlp.config.NlpConfig;
import ca.bc.gov.tno.services.nlp.events.ConsumerPauseEvent;
import ca.bc.gov.tno.services.nlp.events.ConsumerPollEvent;
import ca.bc.gov.tno.services.nlp.events.ConsumerRecordReceivedEvent;
import ca.bc.gov.tno.services.nlp.events.ConsumerResumeEvent;
import ca.bc.gov.tno.services.nlp.events.ErrorEvent;
import ca.bc.gov.tno.services.nlp.events.ServiceStartEvent;
import ca.bc.gov.tno.services.ServiceStatus;

@Async
@Component
public class KafkaNlpConsumer implements ApplicationListener<ServiceStartEvent> {
  private static final Logger logger = LogManager.getLogger(KafkaNlpConsumer.class);

  @Autowired
  private NlpConfig config;

  @Autowired
  private ApplicationEventPublisher applicationEventPublisher;

  @Autowired
  private Global global;

  private KafkaConsumer<String, SourceContent> consumer;

  private List<String> topics;

  private Duration timeout;

  @EventListener
  public void handlePauseEvent(ConsumerPauseEvent event) {
    logger.info("Pause consumer requested");
    global.setStatus(ServiceStatus.pause);
  }

  @EventListener
  public void handleResumeEvent(ConsumerResumeEvent event) {
    logger.info("Resume consumer requested");
    global.setStatus(ServiceStatus.resume);
  }

  /**
   * Create a new instance of a KafkaNlpConsumer object. Creates a new instance of
   * a KafkaConsumer that can be used by the run() method.
   */
  public KafkaNlpConsumer() {
  }

  /**
   * Initialize after constructor
   */
  @PostConstruct
  public void init() {
    topics = Arrays.asList(config.getConsumerTopics().trim().split("\\s*,\\s*"));
    timeout = Duration.ofMillis(config.getPollTimeout());

    var props = new Properties();
    props.put(ConsumerConfig.BOOTSTRAP_SERVERS_CONFIG, config.getBootstrapServers());
    props.put(ConsumerConfig.GROUP_ID_CONFIG, config.getGroupId());
    props.put(ConsumerConfig.KEY_DESERIALIZER_CLASS_CONFIG, StringDeserializer.class);
    props.put(ConsumerConfig.VALUE_DESERIALIZER_CLASS_CONFIG, JsonDeserializer.class);
    props.put(JsonSerializer.TYPE_MAPPINGS,
        String.format("Content:%s, Tag:%s", SourceContent.class.getName(), Tag.class.getName()));
    props.put(JsonDeserializer.KEY_DEFAULT_TYPE, String.class.getName());
    props.put(JsonDeserializer.VALUE_DEFAULT_TYPE, SourceContent.class.getName());
    props.put(ConsumerConfig.ENABLE_AUTO_COMMIT_CONFIG, config.isEnableAutoCommit());
    props.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, config.getAutoOffsetRest());
    consumer = new KafkaConsumer<String, SourceContent>(props);
  }

  @Override
  public void onApplicationEvent(ServiceStartEvent event) {
    logger.info("Consumer start event received.");
    run();
  }

  public void run() {
    try {
      global.setStatus(ServiceStatus.running);
      consumer.subscribe(topics);
      var assignment = consumer.assignment();
      var topic = String.join(", ", topics);

      while (global.getStatus() != ServiceStatus.sleeping) {

        if (global.getStatus() == ServiceStatus.pause) {
          consumer.pause(assignment);
          global.setStatus(ServiceStatus.paused);
        } else if (global.getStatus() == ServiceStatus.resume) {
          consumer.resume(assignment);
          global.setStatus(ServiceStatus.running);
        }

        logger.debug(String.format("Polling: (%s) '%s'", global.getStatus(), topic));
        ConsumerRecords<String, SourceContent> records = consumer.poll(timeout);
        logger.debug(String.format("Received records: (%s)", records.count()));

        var pollEvent = new ConsumerPollEvent(this);
        applicationEventPublisher.publishEvent(pollEvent);
        global.setFailedAttempts(0);

        for (ConsumerRecord<String, SourceContent> record : records) {
          logger.info(String.format("Received record: '%s'", record.key()));
          // TODO: Need to record any failures so that they can be identified and rerun.
          var recordReceivedEvent = new ConsumerRecordReceivedEvent(this, record);
          applicationEventPublisher.publishEvent(recordReceivedEvent);
        }
      }
    } catch (InvalidOffsetException | WakeupException | InterruptException | ArithmeticException ex) {
      global.setStatus(ServiceStatus.sleeping);
      var errorEvent = new ErrorEvent(this, ex);
      applicationEventPublisher.publishEvent(errorEvent);
    }
  }

  @Override
  public void finalize() {
    consumer.close();
  }
}
