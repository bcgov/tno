package ca.bc.gov.tno.models;

import java.time.ZonedDateTime;
import java.util.UUID;

import ca.bc.gov.tno.dal.db.AuditColumns;

public abstract class AuditColumnModel {
  private UUID createdById;
  private String createdBy;
  private ZonedDateTime createdOn;
  private UUID updatedById;
  private String updatedBy;
  private ZonedDateTime updatedOn;
  private long version;

  public AuditColumnModel() {
  }

  public AuditColumnModel(AuditColumns entity) {
    if (entity != null) {
      this.createdById = entity.getCreatedById();
      this.createdBy = entity.getCreatedBy();
      this.createdOn = entity.getCreatedOn();
      this.updatedById = entity.getUpdatedById();
      this.updatedBy = entity.getUpdatedBy();
      this.updatedOn = entity.getUpdatedOn();
      this.version = entity.getVersion();
    }
  }

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
  public ZonedDateTime getCreatedOn() {
    return createdOn;
  }

  /**
   * @param createdOn the createdOn to set
   */
  public void setCreatedOn(ZonedDateTime createdOn) {
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
  public ZonedDateTime getUpdatedOn() {
    return updatedOn;
  }

  /**
   * @param updatedOn the updatedOn to set
   */
  public void setUpdatedOn(ZonedDateTime updatedOn) {
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
