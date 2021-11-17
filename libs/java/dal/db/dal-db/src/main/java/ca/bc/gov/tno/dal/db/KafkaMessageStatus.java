package ca.bc.gov.tno.dal.db;

import java.util.EnumSet;

import ca.bc.gov.tno.IEnumValue;

/**
 * Provides kafka message status to determine if content has been added to a
 * topic.
 */
public enum KafkaMessageStatus implements IEnumValue<Integer> {
  /**
   * Message has been received from data source and is in progress of being
   * ingested. It has not yet been added to TNO.
   */
  InProgress(0),
  /**
   * Message has been received by TNO, but is not searchable.
   */
  Received(1),
  /**
   * Message has been received and transcribed in TNO but is not yet searchable.
   */
  Transcribed(2),
  /**
   * Message has been received and Natural Language Processed in TNO but is not
   * yet searchable.
   */
  NLP(3),
  /**
   * Message has successfully been added to TNO, and is searchable.
   */
  Success(4),
  /**
   * Message has failed to be added to TNO.
   */
  Failed(-1);

  private final int value;

  /**
   * Creates a new instance of an KafkaMessageStatus enum value, initializes with
   * specified value.
   */
  KafkaMessageStatus(final int newValue) {
    value = newValue;
  }

  /**
   * Get the current enum value.
   */
  public Integer getValue() {
    return value;
  }

  /**
   * Get all the kafka message status enum values.
   */
  public static final EnumSet<KafkaMessageStatus> All = EnumSet.allOf(KafkaMessageStatus.class);
}
