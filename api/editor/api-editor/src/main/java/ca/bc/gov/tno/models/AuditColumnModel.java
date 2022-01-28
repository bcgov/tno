package ca.bc.gov.tno.models;

import java.util.Date;
import java.util.UUID;

import ca.bc.gov.tno.dal.db.AuditColumns;

public abstract class AuditColumnModel {
  private UUID createdById;
  private String createdBy;
  private Date createdOn;
  private UUID updatedById;
  private String updatedBy;
  private Date updatedOn;

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
   * @return Date return the createdOn
   */
  public Date getCreatedOn() {
    return createdOn;
  }

  /**
   * @param createdOn the createdOn to set
   */
  public void setCreatedOn(Date createdOn) {
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
   * @return Date return the updatedOn
   */
  public Date getUpdatedOn() {
    return updatedOn;
  }

  /**
   * @param updatedOn the updatedOn to set
   */
  public void setUpdatedOn(Date updatedOn) {
    this.updatedOn = updatedOn;
  }

}
