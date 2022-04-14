package ca.bc.gov.tno.services.models;

import java.io.Serializable;
import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;
import java.util.UUID;

import com.fasterxml.jackson.annotation.JsonFormat;
import com.fasterxml.jackson.databind.annotation.JsonDeserialize;

import ca.bc.gov.tno.services.converters.Settings;
import ca.bc.gov.tno.services.converters.ZonedDateTimeDeserializer;

/**
 * AuditColumns abstract class, provides standardized audit columns for entities
 * in the DB.
 */
public abstract class AuditColumns implements Serializable {
  /**
   * The uid that identifies the user who created the record.
   */
  private UUID createdById = new UUID(0L, 0L);

  /**
   * A name that identifies the user who created the record.
   */
  private String createdBy = "";

  /**
   * When the record was created.
   * Automatically set by the DB.
   */
  // @JsonFormat(shape = JsonFormat.Shape.STRING, pattern =
  // Settings.dateTimeFormat)
  // @JsonDeserialize(using = ZonedDateTimeDeserializer.class)
  private String createdOn = ZonedDateTime.now().format(DateTimeFormatter.ofPattern(Settings.dateTimeFormat));

  /**
   * The uid that identifies the user who updated the record last.
   */
  private UUID updatedById = new UUID(0L, 0L);

  /**
   * The name that identifies the user who updated the record last.
   */
  private String updatedBy = "";

  /**
   * When the record was last updated.
   * Automatically set by the DB.
   */
  // @JsonFormat(shape = JsonFormat.Shape.STRING, pattern =
  // Settings.dateTimeFormat)
  // @JsonDeserialize(using = ZonedDateTimeDeserializer.class)
  private String updatedOn = ZonedDateTime.now().format(DateTimeFormatter.ofPattern(Settings.dateTimeFormat));

  /**
   * Provides concurrency control to enforce optimistic concurrency.
   * Automatically set by the DB.
   */
  private long version;

  /**
   * @return UUID return the createdById
   */
  public UUID getCreatedById() {
    return createdById;
  }

  /**
   * @param createdById the createdById to set
   */
  public void setCreatedById(UUID createdById) {
    this.createdById = createdById;
  }

  /**
   * @return String return the createdBy
   */
  public String getCreatedBy() {
    return createdBy;
  }

  /**
   * @param createdBy the createdBy to set
   */
  public void setCreatedBy(String createdBy) {
    this.createdBy = createdBy;
  }

  /**
   * @return ZonedDateTime return the createdOn
   */
  public String getCreatedOn() {
    return createdOn;
  }

  /**
   * @param createdOn the createdOn to set
   */
  public void setCreatedOn(String createdOn) {
    this.createdOn = createdOn;
  }

  /**
   * @return UUID return the updatedById
   */
  public UUID getUpdatedById() {
    return updatedById;
  }

  /**
   * @param updatedById the updatedById to set
   */
  public void setUpdatedById(UUID updatedById) {
    this.updatedById = updatedById;
  }

  /**
   * @return String return the updatedBy
   */
  public String getUpdatedBy() {
    return updatedBy;
  }

  /**
   * @param updatedBy the updatedBy to set
   */
  public void setUpdatedBy(String updatedBy) {
    this.updatedBy = updatedBy;
  }

  /**
   * @return ZonedDateTime return the updatedOn
   */
  public String getUpdatedOn() {
    return updatedOn;
  }

  /**
   * @param updatedOn the updatedOn to set
   */
  public void setUpdatedOn(String updatedOn) {
    this.updatedOn = updatedOn;
  }

  /**
   * @return long return the version
   */
  public long getVersion() {
    return version;
  }

  /**
   * @param version the version to set
   */
  public void setVersion(long version) {
    this.version = version;
  }

}
