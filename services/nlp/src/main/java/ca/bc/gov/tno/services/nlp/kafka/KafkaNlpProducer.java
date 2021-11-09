package ca.bc.gov.tno.services.nlp.kafka;

import java.util.Properties;
import java.util.concurrent.ExecutionException;

import javax.annotation.PostConstruct;

import org.apache.kafka.clients.producer.KafkaProducer;
import org.apache.kafka.clients.producer.ProducerConfig;
import org.apache.kafka.clients.producer.ProducerRecord;
import org.apache.kafka.common.serialization.StringSerializer;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;
import org.springframework.kafka.support.serializer.JsonSerializer;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.dal.db.KafkaMessageStatus;
import ca.bc.gov.tno.dal.db.entities.ContentReferencePK;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentReferenceService;
import ca.bc.gov.tno.models.NlpContent;
import ca.bc.gov.tno.services.nlp.config.NlpConfig;
import ca.bc.gov.tno.services.nlp.events.ContentReadyEvent;
import ca.bc.gov.tno.services.nlp.events.ErrorEvent;

/**
 * KafkaNlpProducer class, provides a process that pushes messages to Kafka
 * topics.
 */
@Async
@Component
public class KafkaNlpProducer implements ApplicationListener<ContentReadyEvent> {
  private static final Logger logger = LogManager.getLogger(KafkaNlpProducer.class);

  @Autowired
  private ApplicationEventPublisher applicationEventPublisher;

  @Autowired
  private NlpConfig config;

  @Autowired
  private IContentReferenceService contentReferenceService;

  private KafkaProducer<String, NlpContent> producer;

  /**
   * Create a new instance of a KafkaNlpProducer object.
   */
  public KafkaNlpProducer() {
  }

  /**
   * Initialize after constructor. Creates a new instance of a KafkaProducer that
   * can be used by the send() method.
   */
  @PostConstruct
  public void init() {
    var props = new Properties();
    props.put(ProducerConfig.CLIENT_ID_CONFIG, config.getClientId());
    props.put(ProducerConfig.BOOTSTRAP_SERVERS_CONFIG, config.getBootstrapServers());
    props.put(ProducerConfig.KEY_SERIALIZER_CLASS_CONFIG, StringSerializer.class);
    props.put(ProducerConfig.VALUE_SERIALIZER_CLASS_CONFIG, JsonSerializer.class);
    props.put(ProducerConfig.ACKS_CONFIG, "all");
    props.put(ProducerConfig.ENABLE_IDEMPOTENCE_CONFIG, true);
    props.put(ProducerConfig.MAX_IN_FLIGHT_REQUESTS_PER_CONNECTION, 5);
    props.put(ProducerConfig.RETRIES_CONFIG, 10000000);
    props.put(ProducerConfig.BATCH_SIZE_CONFIG, 16384);
    props.put(ProducerConfig.LINGER_MS_CONFIG, 1);
    props.put(ProducerConfig.BUFFER_MEMORY_CONFIG, 33554432);
    producer = new KafkaProducer<String, NlpContent>(props);
  }

  @Override
  public void onApplicationEvent(ContentReadyEvent event) {
    var content = event.getContent();
    try {
      send(content);
    } catch (InterruptedException | ExecutionException ex) {
      var errorEvent = new ErrorEvent(this, ex);
      applicationEventPublisher.publishEvent(errorEvent);
      producer.flush();
    }
  }

  /**
   * Push the content stub into Kafka.
   * 
   * @param content
   * @throws ExecutionException
   * @throws InterruptedException
   */
  public void send(NlpContent content) throws InterruptedException, ExecutionException {
    var topic = config.getProducerTopic();
    var key = String.format("%s-%s", content.getSource(), content.getUid());
    logger.info(String.format("NLP content sending: '%s', topic: %s", key, topic));
    var response = producer.send(new ProducerRecord<String, NlpContent>(topic, key, content));
    var record = response.get();
    logger.info(String.format("NLP content sent: '%s', topic: %s, partition: %s, offset: %s", key, topic,
        record.partition(), record.offset()));

    // Update the content reference status.
    var rContentReference = contentReferenceService
        .findById(new ContentReferencePK(content.getSource(), content.getUid()));
    if (rContentReference.isPresent()) {
      var contentReference = rContentReference.get();
      contentReference.setStatus(KafkaMessageStatus.NLP);
      contentReferenceService.update(contentReference);
      logger.info(
          String.format("Content reference updated with NLP: '%s', source: %s, topic: %s, partition: %s, offset: %s",
              content.getUid(), content.getSource(), content.getTopic(), content.getPartition(), content.getOffset()));
    } else {
      logger.warn(String.format("Content reference missing: '%s', source: %s, topic: %s, partition: %s, offset: %s",
          content.getUid(), content.getSource(), content.getTopic(), content.getPartition(), content.getOffset()));
    }
  }

  @Override
  public void finalize() {
    producer.flush();
    producer.close();
  }
}
