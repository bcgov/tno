package ca.bc.gov.tno.services.elastic.services;

import java.util.stream.Collectors;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.dal.db.KafkaMessageStatus;
import ca.bc.gov.tno.dal.db.entities.ContentReferencePK;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentReferenceService;
import ca.bc.gov.tno.dal.elastic.models.Kafka;
import ca.bc.gov.tno.dal.elastic.models.Source;
import ca.bc.gov.tno.dal.elastic.models.Story;
import ca.bc.gov.tno.dal.elastic.models.Tag;
import ca.bc.gov.tno.dal.elastic.services.StoryService;
import ca.bc.gov.tno.models.NlpContent;
import ca.bc.gov.tno.services.kafka.events.ConsumerRecordReceivedEvent;
import ca.bc.gov.tno.services.events.ErrorEvent;

/**
 * ElasticService class, provides an event listener that saves NLP content into
 * Elasticsearch.
 */
@Service
public class ElasticService implements ApplicationListener<ConsumerRecordReceivedEvent<String, NlpContent>> {
  private static final Logger logger = LogManager.getLogger(ElasticService.class);

  private final ApplicationEventPublisher applicationEventPublisher;
  private final IContentReferenceService dataService;
  private final StoryService elasticService;

  /**
   * Creates a new instances of a ElasticService object, initializes with
   * specified parameters.
   * 
   * @param applicationEventPublisher
   * @param dataService
   * @param elasticService
   */
  @Autowired
  public ElasticService(final ApplicationEventPublisher applicationEventPublisher,
      final IContentReferenceService dataService, final StoryService elasticService) {
    this.applicationEventPublisher = applicationEventPublisher;
    this.dataService = dataService;
    this.elasticService = elasticService;
  }

  /**
   * Event listener will save the story record in elasticsearch.
   * 
   * @param event
   */
  @Override
  public void onApplicationEvent(ConsumerRecordReceivedEvent<String, NlpContent> event) {
    var record = event.getRecord();
    var content = record.value();
    var story = createStory(content);

    try {
      // TODO: Handle updating content. Presently this will attempt to insert a new
      // record.
      logger.debug(String.format("Sending story to index: %s", story.getId()));
      story = elasticService.save(story);
      logger.info(String.format("Story saved in index: %s", story.getId()));

      // Update content reference in TNO DB.
      var rContentReference = dataService.findById(new ContentReferencePK(content.getSource(), content.getUid()));
      if (rContentReference.isPresent()) {
        var contentReference = rContentReference.get();
        contentReference.setStatus(KafkaMessageStatus.Success);

        logger.debug(String.format("Updating content reference: %s:%s", content.getSource(), content.getUid()));
        contentReference = dataService.update(contentReference);
        logger.info(String.format("Updated content reference: %s:%s", content.getSource(), content.getUid()));
      } else {
        logger.warn(String.format("Content reference does not exist: %s:%s", content.getSource(), content.getUid()));
      }
    } catch (Exception ex) {
      var errorEvent = new ErrorEvent(this, ex);
      applicationEventPublisher.publishEvent(errorEvent);
    }
  }

  /**
   * Create a new instance of a Story from the specified 'content'.
   * 
   * @param content A NlpContent object.
   * @return A new instance of a Story.
   */
  private Story createStory(NlpContent content) {
    var kafka = new Kafka(content.getTopic(), content.getPartition(), content.getOffset());
    var source = new Source(content.getSource(), content.getUid());
    source.setLink(content.getLink());
    source.setStreamUrl(content.getStreamUrl());
    source.setFilePath(content.getFilePath());
    var story = new Story(kafka, source, content.getType().getValue(), content.getTitle());
    story.setAuthor(content.getAuthor());
    story.setLanguage(content.getLanguage());
    story.setCopyright(content.getCopyright());
    story.setSummary(content.getSummary());
    story.setUpdatedOn(content.getUpdatedOn());
    story.setPublishedOn(content.getPublishedOn());
    story.setTags(
        content.getTags().stream().map(t -> new Tag(t.getKey().getValue(), t.getValue())).collect(Collectors.toList()));
    return story;
  }

}
