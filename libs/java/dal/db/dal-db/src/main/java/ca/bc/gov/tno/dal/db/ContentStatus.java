package ca.bc.gov.tno.dal.db;

import java.util.EnumSet;

import ca.bc.gov.tno.IEnumValue;

/**
 * Provides content status to determine the stage the content is in.
 */
public enum ContentStatus implements IEnumValue<Integer> {
  /**
   * Content has been received from data source and is in progress of being
   * ingested. It has not yet been added to TNO.
   */
  InProgress(0),
  /**
   * Content has been received by TNO, but is not searchable.
   */
  Received(1),
  /**
   * Content has been received and transcribed in TNO but is not yet searchable.
   */
  Transcribed(2),
  /**
   * Content has been received and Natural Language Processed in TNO but is not
   * yet searchable.
   */
  NLP(3),
  /**
   * Content has successfully been added to TNO, and is searchable.
   */
  Success(4),
  /**
   * Content has been published.
   */
  Published(5),
  /**
   * Content has failed to be added to TNO.
   */
  Failed(-1);

  private final int value;

  /**
   * Creates a new instance of an ContentStatus enum value, initializes with
   * specified value.
   */
  ContentStatus(final int newValue) {
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
  public static final EnumSet<ContentStatus> All = EnumSet.allOf(ContentStatus.class);
}
