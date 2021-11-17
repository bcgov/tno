package ca.bc.gov.tno.services;

import java.util.EnumSet;

import ca.bc.gov.tno.IEnumValue;

/**
 * ServiceStatus enum, provides the service status options.
 */
public enum ServiceStatus implements IEnumValue<String> {
  /**
   * Service is running.
   */
  running("running"),
  /**
   * Service has been requested to pause.
   */
  pause("pause"),
  /**
   * Service is paused.
   */
  paused("paused"),
  /**
   * Service has been requested to resume from pause.
   */
  resume("resume"),
  /**
   * Service is sleeping.
   */
  sleeping("sleeping");

  private final String value;

  /**
   * Creates a new instance of a ServiceStatus enum, initializes with specified
   * parameter.
   * 
   * @param newValue The current value of this enum.
   */
  ServiceStatus(final String newValue) {
    value = newValue;
  }

  /**
   * Get the enum value name.
   */
  public String getValue() {
    return value;
  }

  /**
   * Get all of the service status enum values.
   */
  public static final EnumSet<ServiceStatus> All = EnumSet.allOf(ServiceStatus.class);
}
