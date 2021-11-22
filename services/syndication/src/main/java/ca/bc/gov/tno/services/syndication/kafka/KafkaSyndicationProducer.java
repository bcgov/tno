package ca.bc.gov.tno.services.syndication.kafka;

import java.util.ArrayList;
import java.util.Date;
import java.util.Properties;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.Future;
import java.util.concurrent.TimeUnit;
import java.util.stream.Collectors;

import com.rometools.rome.feed.synd.SyndEntry;

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

import ca.bc.gov.tno.dal.db.KafkaMessageStatus;
import ca.bc.gov.tno.dal.db.entities.ContentReference;
import ca.bc.gov.tno.dal.db.entities.ContentReferencePK;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentReferenceService;
import ca.bc.gov.tno.models.SourceContent;
import ca.bc.gov.tno.models.Tag;
import ca.bc.gov.tno.services.data.events.TransactionCompleteEvent;
import ca.bc.gov.tno.services.events.ErrorEvent;
import ca.bc.gov.tno.services.kafka.config.KafkaProducerConfig;
import ca.bc.gov.tno.services.syndication.events.ProducerSendEvent;
import ca.bc.gov.tno.services.syndication.models.PublishedRecord;
import ca.bc.gov.tno.TagKey;

/**
 * KafkaSyndicationProducer class, provides a process that pushes messages to
 * Kafka topics.
 */
@Async
@Component
public class KafkaSyndicationProducer implements ApplicationListener<ProducerSendEvent> {
  private static final Logger logger = LogManager.getLogger(KafkaSyndicationProducer.class);

  private final ApplicationEventPublisher eventPublisher;
  private final KafkaProducerConfig kafkaConfig;
  private final IContentReferenceService contentReferenceService;
  private final KafkaProducer<String, SourceContent> producer;

  /**
   * Create a new instance of a KafkaSyndicationProducer object.
   */
  @Autowired
  public KafkaSyndicationProducer(final KafkaProducerConfig config, final IContentReferenceService service,
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
      var config = event.getConfig();
      send(event);

      var doneEvent = new TransactionCompleteEvent<>(event.getSource(), config);
      eventPublisher.publishEvent(doneEvent);
    } catch (InterruptedException | ExecutionException ex) {
      var errorEvent = new ErrorEvent(this, ex);
      eventPublisher.publishEvent(errorEvent);
      producer.flush();
    }
  }

  /**
   * Push the content stub into Kafka.
   * 
   * @param event
   * @throws ExecutionException
   * @throws InterruptedException
   */
  public void send(ProducerSendEvent event) throws InterruptedException, ExecutionException {
    var feed = event.getData();
    var responses = new ArrayList<PublishedRecord>();

    for (var entry : feed.getEntries()) {
      var record = handleDuplication(event, entry);
      if (record != null)
        responses.add(record);
    }

    var futureIter = responses.iterator();

    // TODO: Handle errors that occur on send.
    while (futureIter.hasNext()) {
      var future = futureIter.next();
      var record = future.getFutureRecordMetadata().get();
      var content = future.getContentReference();

      logger.info(String.format("Syndication content added: '%s', topic: %s, partition: %s, offset: %s",
          content.getUid(), record.topic(), record.partition(), record.offset()));

      try {
        // TODO: It's possible a failure here will result in the record being stuck "In
        // Progress". This needs to be resolved.

        // Only update the content reference if it was in progress.
        // An update won't change the status.
        if (content.getStatus() == KafkaMessageStatus.InProgress) {
          content.setPartition(record.partition());
          content.setOffset(record.offset());
          content.setStatus(KafkaMessageStatus.Received);
          contentReferenceService.update(content);
        }
      } catch (Exception ex) {
        // Hopefully an error on one entry won't stop all other entries.
        var errorEvent = new ErrorEvent(this, ex);
        eventPublisher.publishEvent(errorEvent);
      }

      futureIter.remove();
    }
  }

  /**
   * Compare each feed entry with the content reference stored in TNO to determine
   * if it currently being ingested or whether it is a duplicate. If the entry is
   * a duplicate, determine if it has been updated. If so, push to TNO with an
   * update.
   * 
   * @param event
   * @param entry
   * @return A new PublishRecord object or null to stop duplication.
   */
  private PublishedRecord handleDuplication(ProducerSendEvent event, SyndEntry entry) {
    var config = event.getConfig();
    var source = config.getId();
    var topic = config.getTopic();
    var uid = entry.getUri();

    try {
      // Ensure no duplicates are imported.
      var rContentReference = contentReferenceService.findById(new ContentReferencePK(source, uid));
      if (rContentReference.isPresent()) {
        // The content reference already exists,
        var contentReference = rContentReference.get();

        // TODO: verify a hash of the content to ensure it has changed. This may be slow
        // however, but would ensure the content was physically updated.

        // If another process has it in progress only attempt to do an import if it's
        // more than an hour old.
        // Assumption is that it is stuck.
        if (contentReference.getStatus() == KafkaMessageStatus.InProgress) {
          var diffMinutes = new Date(System.currentTimeMillis()).getTime() - contentReference.getUpdatedOn().getTime();
          var diff = TimeUnit.MINUTES.convert(diffMinutes, TimeUnit.MINUTES);
          if (diff >= 60) {
            logger.warn(String.format("Stuck content identified: '%s', topic: %s", uid, topic));
            contentReference.setPublishedOn(entry.getPublishedDate());
            contentReference.setSourceUpdatedOn(entry.getUpdatedDate());
            contentReference = contentReferenceService.update(contentReference);
            return new PublishedRecord(publishEntry(event, entry), contentReference);
          }
        } else {
          var updatedOn = contentReference.getSourceUpdatedOn();
          var publishedOn = contentReference.getPublishedOn();
          // If the source feed entry has been updated or republished it will need to get
          // pushed to Kafka and the reference will need to be updated.
          if ((updatedOn != null && updatedOn.before(entry.getUpdatedDate()))
              || (publishedOn != null && publishedOn.before(entry.getPublishedDate()))) {
            logger.info(String.format("Updated content identified: '%s', topic: %s, partition: %s, offset: %s", uid,
                topic, contentReference.getPartition(), contentReference.getOffset()));
            contentReference.setPublishedOn(publishedOn);
            contentReference.setSourceUpdatedOn(entry.getUpdatedDate());
            contentReference = contentReferenceService.update(contentReference);
            return new PublishedRecord(publishEntry(event, entry), contentReference);
          }
        }

        logger.debug(String.format("Duplicate content identified: '%s', topic: %s, partition: %s, offset: %s", uid,
            topic, contentReference.getPartition(), contentReference.getOffset()));
      } else {
        // Add a reference to the new content immediately so that that subsequent
        // attempts will know it is in progress and then they will not attempt to
        // publish the same content.
        logger.info(String.format("New content identified: '%s', topic: %s", uid, topic));
        var contentReference = new ContentReference(source, uid, topic);
        contentReference.setPublishedOn(entry.getPublishedDate());
        contentReference.setSourceUpdatedOn(entry.getUpdatedDate());
        contentReference = contentReferenceService.add(contentReference);
        return new PublishedRecord(publishEntry(event, entry), contentReference);
      }
    } catch (Exception ex) {
      // TODO: It's possible a failure here will result in the record being imported
      // multiple times. This needs to be handled.
      // Hopefully an error on one entry won't stop all other entries.
      var errorEvent = new ErrorEvent(this, ex);
      eventPublisher.publishEvent(errorEvent);
    }

    return null;
  }

  /**
   * Send the content to Kafka topic.
   * 
   * @param event
   * @param entry
   * @return Future{RecordMetadata} object.
   */
  private Future<RecordMetadata> publishEntry(ProducerSendEvent event, SyndEntry entry) {
    try {
      var config = event.getConfig();
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
   * Create a SourceContent object from the syndication feed entry.
   * 
   * @param event
   * @param entry
   * @return new instance of a SourceContent object.
   */
  private SourceContent generateContent(ProducerSendEvent event, SyndEntry entry) {
    var feed = event.getData();
    var config = event.getConfig();
    var source = entry.getSource() != null ? entry.getSource() : feed;

    var content = new SourceContent();
    content.setSource(config.getId());
    content.setUid(entry.getUri());
    content.setLink(entry.getLink());
    content.setTitle(entry.getTitle());
    content.setAuthor(entry.getAuthor());
    content.setPublishedOn(entry.getPublishedDate());
    content.setUpdatedOn(entry.getUpdatedDate());
    content.setCopyright(source.getCopyright());
    content.setLanguage(source.getLanguage()); // TODO: Clean up languages.
    var description = entry.getDescription();
    if (description != null)
      content.setSummary(description.getValue());
    var body = entry.getContents();
    if (body != null)
      content.setBody(body.stream().map(c -> c.getValue()).collect(Collectors.joining()));
    var categories = entry.getCategories();
    if (categories != null) {
      var tags = content.getTags();
      categories.forEach(c -> {
        var tag = new Tag(TagKey.category, c.getName());
        tags.add(tag);
      });
    }

    return content;
  }

  @Override
  public void finalize() {
    producer.flush();
    producer.close();
  }
}
