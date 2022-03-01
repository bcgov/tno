package ca.bc.gov.tno.areas.editor.models;

import ca.bc.gov.tno.dal.db.entities.License;
import ca.bc.gov.tno.models.AuditColumnModel;

public class LicenseModel extends AuditColumnModel {
  /**
   * Primary key to identify the license.
   */
  private int id;

  /**
   * A unique name to identify the license.
   */
  private String name;

  /**
   * A description of the license.
   */
  private String description = "";

  /**
   * Whether this record is enabled or disabled.
   */
  private boolean enabled = true;

  /**
   * The number of days content is allowed to be kept before it must be purged (0
   * = forever).
   */
  private int ttl;

  /**
   * A collection of data sources that belong to this license.
   */
  // private List<DataSource> data_sources = new ArrayList<>();

  /**
   * A collection of content that belong to this license.
   */
  // private List<Content> contents = new ArrayList<>();

  /**
   * A collection of data sources of this type.
   */
  // private List<DataSource> data_sources = new ArrayList<>();

  public LicenseModel() {
  }

  public LicenseModel(License entity) {
    super(entity);

    if (entity != null) {
      this.id = entity.getId();
      this.name = entity.getName();
      this.description = entity.getDescription();
      this.enabled = entity.isEnabled();
      this.ttl = entity.getTtl();
    }
  }

  /**
   * @return int return the id
   */
  public int getId() {
    return id;
  }

  /**
   * @param id the id to set
   */
  public void setId(int id) {
    this.id = id;
  }

  /**
   * @return String return the name
   */
  public String getName() {
    return name;
  }

  /**
   * @param name the name to set
   */
  public void setName(String name) {
    this.name = name;
  }

  /**
   * @return String return the description
   */
  public String getDescription() {
    return description;
  }

  /**
   * @param description the description to set
   */
  public void setDescription(String description) {
    this.description = description;
  }

  /**
   * @return boolean return the enabled
   */
  public boolean isEnabled() {
    return enabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setEnabled(boolean enabled) {
    this.enabled = enabled;
  }

  /**
   * @return int return the ttl
   */
  public int getTtl() {
    return ttl;
  }

  /**
   * @param ttl the ttl to set
   */
  public void setTtl(int ttl) {
    this.ttl = ttl;
  }

}
