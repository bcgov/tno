package ca.bc.gov.tno.dal.db;

import ca.bc.gov.tno.dal.db.entities.IEnumValue;

/**
 * Provides kafka message status to determine if content has been added to a
 * topic.
 * 
 * InProgress: The message has been sent to Kafka but a response has not yet
 * been received.
 * 
 * Success: Kafka has stored the message in the topic.
 * 
 * Failed: Kafka was unable to store the message in the topic.
 */
public enum KafkaMessageStatus implements IEnumValue {
  InProgress(0), Success(1), Failed(2);

  private final int value;

  KafkaMessageStatus(final int newValue) {
    value = newValue;
  }

  public int getValue() {
    return value;
  }
}
