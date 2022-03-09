package ca.bc.gov.tno.services.audio.kafka;

import java.time.ZonedDateTime;
import java.util.Date;
import java.util.Properties;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.Future;

import org.apache.kafka.clients.producer.KafkaProducer;
import org.apache.kafka.clients.producer.ProducerConfig;
import org.apache.kafka.clients.producer.ProducerRecord;
import org.apache.kafka.clients.producer.RecordMetadata;
import org.apache.kafka.common.serialization.StringSerializer;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;
import org.springframework.kafka.support.serializer.JsonSerializer;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.dal.db.WorkflowStatus;
import ca.bc.gov.tno.dal.db.entities.ContentReference;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentReferenceService;
import ca.bc.gov.tno.models.SourceContent;
import ca.bc.gov.tno.services.data.events.TransactionCompleteEvent;
import ca.bc.gov.tno.services.events.ErrorEvent;
import ca.bc.gov.tno.services.kafka.config.KafkaProducerConfig;
import ca.bc.gov.tno.services.audio.events.ProducerSendEvent;
import ca.bc.gov.tno.services.audio.models.PublishedRecord;

import ca.bc.gov.tno.ContentType;

/**
 * KafkaAudioProducer class, provides a process that pushes messages to
 * Kafka topics.
 */
@Async
@Component
public class KafkaAudioProducer implements ApplicationListener<ProducerSendEvent> {
  private static final Logger logger = LogManager.getLogger(KafkaAudioProducer.class);

  private final ApplicationEventPublisher eventPublisher;
  private final KafkaProducerConfig kafkaConfig;
  private final IContentReferenceService contentReferenceService;
  private final KafkaProducer<String, SourceContent> producer;

  /**
   * Create a new instance of a KafkaAudioProducer object.
   */
  @Autowired
  public KafkaAudioProducer(final KafkaProducerConfig config, final IContentReferenceService service,
      final ApplicationEventPublisher eventPublisher) {
    this.kafkaConfig = config;
    this.contentReferenceService = service;
    this.eventPublisher = eventPublisher;

    var props = new Properties();
    props.put(ProducerConfig.CLIENT_ID_CONFIG, kafkaConfig.getClientId());
    props.put(ProducerConfig.BOOTSTRAP_SERVERS_CONFIG, kafkaConfig.getBootstrapServers());
    props.put(ProducerConfig.KEY_SERIALIZER_CLASS_CONFIG, StringSerializer.class);
    props.put(ProducerConfig.VALUE_SERIALIZER_CLASS_CONFIG, JsonSerializer.class);
    props.put(ProducerConfig.ACKS_CONFIG, "all");
    props.put(ProducerConfig.ENABLE_IDEMPOTENCE_CONFIG, true);
    props.put(ProducerConfig.MAX_IN_FLIGHT_REQUESTS_PER_CONNECTION, 5);
    props.put(ProducerConfig.RETRIES_CONFIG, 10000000);
    props.put(ProducerConfig.BATCH_SIZE_CONFIG, 16384);
    props.put(ProducerConfig.LINGER_MS_CONFIG, 1);
    props.put(ProducerConfig.BUFFER_MEMORY_CONFIG, 33554432);
    this.producer = new KafkaProducer<String, SourceContent>(props);
  }

  /**
   * Send the message to Kafka.
   */
  @Override
  public void onApplicationEvent(ProducerSendEvent event) {
    try {
      var dataSource = event.getDataSource();
      var schedule = event.getSchedule();
      send(event);

      var doneEvent = new TransactionCompleteEvent(event.getSource(), dataSource, schedule);
      eventPublisher.publishEvent(doneEvent);
    } catch (InterruptedException | ExecutionException ex) {
      var errorEvent = new ErrorEvent(this, ex);
      eventPublisher.publishEvent(errorEvent);
      producer.flush();
    }
  }

  /**
   * Push the content stub into Kafka if clip extraction was successful.
   * 
   * @param event
   * @throws ExecutionException
   * @throws InterruptedException
   */
  public void send(ProducerSendEvent event) throws InterruptedException, ExecutionException {
    var clip = event.getData();

    var config = event.getDataSource();
    var source = config.getId();
    var topic = config.getTopic();

    try {
      if(config.getClipSuccess()) {
        logger.info(String.format("New audio content extracted: '%s', topic: %s", clip, topic));
        var contentReference = new ContentReference(source, clip, topic);
        contentReference.setPublishedOn(ZonedDateTime.now());
        contentReference.setSourceUpdatedOn(ZonedDateTime.now());

        var record = new PublishedRecord(publishEntry(event, clip), contentReference);
        var meta = record.getFutureRecordMetadata().get();

        contentReference.setPartition(meta.partition());
        contentReference.setOffset(meta.offset());
        contentReference.setStatus(WorkflowStatus.Received);

        logger.info(String.format("Audio content added: '%s', topic: %s, partition: %s, offset: %s",
        clip, meta.topic(), meta.partition(), meta.offset()));

        contentReference = contentReferenceService.add(contentReference);
      }
    } catch (Exception ex) {
      // Hopefully an error on one entry won't stop all other entries.
      var errorEvent = new ErrorEvent(this, ex);
      eventPublisher.publishEvent(errorEvent);
    }
  }

  /**
   * Send the content to Kafka topic.
   * 
   * @param event
   * @param entry
   * @return Future{RecordMetadata} object.
   */
  private Future<RecordMetadata> publishEntry(ProducerSendEvent event, String entry) {
    try {
      var config = event.getDataSource();
      var topic = config.getTopic();
      var content = generateContent(event, entry);
      var key = String.format("%s-%s", content.getSource(), content.getUid());
      logger.info(String.format("Sending content: '%s', topic: %s", key, topic));
      return producer.send(new ProducerRecord<String, SourceContent>(topic, key, content));
    } catch (Exception ex) {
      var errorEvent = new ErrorEvent(this, ex);
      eventPublisher.publishEvent(errorEvent);
    }
    return CompletableFuture.completedFuture(null);
  }

  /**
   * Create a SourceContent object from the audio feed entry.
   * 
   * @param event
   * @param entry
   * @return new instance of a SourceContent object.
   */
  private SourceContent generateContent(ProducerSendEvent event, String entry) {

    var config = event.getDataSource();

    var content = new SourceContent();
    content.setSource(entry);
    content.setUid("none");
    content.setLink("none");
    content.setTitle(config.getId());
    content.setAuthor(config.getId());
    content.setPublishedOn(new Date());
    content.setUpdatedOn(new Date());
    content.setCopyright(config.getId());
    content.setSummary("Streaming audio clip from station: " + config.getId());
    content.setFilePath(entry);
    content.setBody("none");
    content.setType(ContentType.audio);
    content.setLanguage("en"); // TODO: Clean up languages.

    return content;
  }

  @Override
  public void finalize() {
    producer.flush();
    producer.close();
  }
}
