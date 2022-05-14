package ca.bc.gov.tno.services.syndication.models;

import java.util.concurrent.Future;

import org.apache.kafka.clients.producer.RecordMetadata;

import ca.bc.gov.tno.services.models.ContentReference;

/**
 * PublishedRecord class, provides a container for holding the published record.
 */
public class PublishedRecord {
  private Future<RecordMetadata> futureRecordMetadata;
  private ContentReference contentReference;

  /**
   * Creates a new instance of a PublishedRecord object, initializes with
   * specified parameters.
   * 
   * @param future  The future async action that is publishing content to Kafka.
   * @param content The content reference.
   */
  public PublishedRecord(Future<RecordMetadata> future, ContentReference content) {
    if (future == null)
      throw new IllegalArgumentException("Parameter 'future' is required.");
    if (content == null)
      throw new IllegalArgumentException("Parameter 'content' is required.");

    this.futureRecordMetadata = future;
    this.contentReference = content;
  }

  public Future<RecordMetadata> getFutureRecordMetadata() {
    return futureRecordMetadata;
  }

  public ContentReference getContentReference() {
    return contentReference;
  }
}
