package ca.bc.gov.tno.areas.editor.models;

import java.util.Date;
import java.util.HashMap;
import java.util.Map;

import ca.bc.gov.tno.dal.db.entities.DataSource;

public class DataSourceModel {
  /**
   * Primary key to identify the data source.
   */
  private int id;

  /**
   * A unique name to identify the data source.
   */
  private String name;

  /**
   * A unique abbreviation to identify the data source. This is used in the
   * content reference table.
   */
  private String code;

  /**
   * A description of the data source.
   */
  private String description = "";

  /**
   * Whether this record is enabled or disabled.
   */
  private boolean enabled = true;

  /**
   * Foreign key to the media type.
   */
  private int mediaTypeId;

  /**
   * The media type reference.
   */
  private MediaTypeModel mediaType;

  /**
   * Foreign key to the license.
   */
  private int licenseId;

  /**
   * The license reference.
   */
  private LicenseModel license;

  /**
   * Foreign key to the schedule.
   */
  private int scheduleId;

  /**
   * The schedule reference.
   */
  private ScheduleModel schedule;

  /**
   * The Kafka topic that content will be pushed into.
   */
  private String topic;

  /**
   * The date and time this data source was successfully ingested on.
   */
  private Date lastRanOn;

  /**
   * JSON configuration values for the ingestion services.
   */
  private Map<String, Object> connection = new HashMap<>();

  public DataSourceModel() {
  }

  public DataSourceModel(DataSource entity) {
    this.id = entity.getId();
    this.name = entity.getName();
    this.description = entity.getDescription();
    this.enabled = entity.isEnabled();
    this.mediaTypeId = entity.getMediaTypeId();
    this.mediaType = new MediaTypeModel(entity.getMediaType());
    this.licenseId = entity.getLicenseId();
    this.license = new LicenseModel(entity.getLicense());
    this.scheduleId = entity.getScheduleId();
    this.schedule = new ScheduleModel(entity.getSchedule());
    this.topic = entity.getTopic();
    this.lastRanOn = entity.getLastRanOn();
    this.connection = entity.getConnection();
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

}
