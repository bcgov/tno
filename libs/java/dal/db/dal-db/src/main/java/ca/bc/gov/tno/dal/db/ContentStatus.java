package ca.bc.gov.tno.dal.db;

import java.util.EnumSet;

import ca.bc.gov.tno.IEnumValue;

/**
 * Provides content status to determine the stage the content is in.
 * Content status represents either what process should be performed.
 */
public enum ContentStatus implements IEnumValue<Integer> {
  /**
   * Content will not be published.
   */
  Draft(0),
  /**
   * Content added to queue to publish.
   */
  Publish(1),
  /**
   * Content has been published.
   */
  Published(2),
  /**
   * Content has been requested to be unpublished.
   */
  Unpublish(3),
  /**
   * Content has been unpublished.
   */
  Unpublished(4);

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
