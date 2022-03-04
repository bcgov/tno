package ca.bc.gov.tno.dal.db.entities;

import java.time.ZonedDateTime;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.persistence.CascadeType;
import javax.persistence.Column;
import javax.persistence.Convert;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.JoinColumn;
import javax.persistence.ManyToOne;
import javax.persistence.OneToMany;
import javax.persistence.SequenceGenerator;
import javax.persistence.Table;

import com.fasterxml.jackson.annotation.JsonBackReference;
import com.fasterxml.jackson.annotation.JsonFormat;

import ca.bc.gov.tno.dal.db.AuditColumns;
import ca.bc.gov.tno.dal.db.converters.HashMapToStringConverter;
import ca.bc.gov.tno.dal.db.services.Settings;

/**
 * DataSource class, defines a set of data, how often it is requested, the
 * length of time it will be stored, and the Kafka topic it will be stored in.
 */
@Entity
@Table(name = "data_source", schema = "public")
public class DataSource extends AuditColumns {
  /**
   * Primary key to identify the data source.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_data_source")
  @SequenceGenerator(name = "seq_data_source", allocationSize = 1)
  @Column(name = "id", nullable = false)
  private int id;

  /**
   * A unique name to identify the data source.
   */
  @Column(name = "name", nullable = false)
  private String name;

  /**
   * A unique abbreviation to identify the data source. This is used in the
   * content reference table.
   */
  @Column(name = "code", nullable = false)
  private String code;

  /**
   * A description of the data source.
   */
  @Column(name = "description")
  private String description = "";

  /**
   * Whether this record is enabled or disabled.
   */
  @Column(name = "is_enabled", nullable = false)
  private boolean enabled = true;

  /**
   * Foreign key to the media type.
   */
  @Column(name = "media_type_id", nullable = false)
  private int mediaTypeId;

  /**
   * The media type reference.
   */
  @JsonBackReference("mediaType")
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "media_type_id", insertable = false, updatable = false)
  private MediaType mediaType;

  /**
   * Foreign key to the license.
   */
  @Column(name = "license_id", nullable = false)
  private int licenseId;

  /**
   * The license reference.
   */
  @JsonBackReference("license")
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "license_id", insertable = false, updatable = false)
  private License license;

  /**
   * The Kafka topic that content will be pushed into.
   */
  @Column(name = "topic", nullable = false)
  private String topic;

  /**
   * The date and time this data source was successfully ingested on.
   */
  @JsonFormat(shape = JsonFormat.Shape.STRING, pattern = Settings.dateTimeFormat, timezone = "UTC")
  @Column(name = "last_ran_on")
  private ZonedDateTime lastRanOn;

  /**
   * JSON configuration values for the ingestion services.
   */
  @Convert(converter = HashMapToStringConverter.class)
  @Column(name = "connection", nullable = false, columnDefinition = "text") // TODO: Switch to JSON in DB
  private Map<String, Object> connection = new HashMap<>();

  /**
   * Foreign key to parent data source.
   */
  @Column(name = "parent_id", nullable = true)
  private Integer parentId;

  /**
   * Reference to the parent data source.
   */
  @JsonBackReference("parent")
  @ManyToOne(fetch = FetchType.EAGER)
  @JoinColumn(name = "parent_id", insertable = false, updatable = false)
  private DataSource parent;

  /**
   * A collection of data source schedules linked to this data source.
   */
  @OneToMany(mappedBy = "dataSource", fetch = FetchType.LAZY, cascade = { CascadeType.MERGE, CascadeType.PERSIST })
  private List<DataSourceSchedule> dataSourceSchedules = new ArrayList<>();

  /**
   * Creates a new instance of a DataSource object.
   */
  public DataSource() {
  }

  /**
   * Creates a new instance of a DataSource object, initializes with specified
   * parameters.
   * 
   * @param id         The primary key
   * @param name       The unique name
   * @param abbr       The unique abbreviation
   * @param mediaType  Foreign key to the media type
   * @param license    Foreign key to the license
   * @param topic      The Kafka topic
   * @param connection The connection information
   */
  public DataSource(int id, String name, String abbr, MediaType mediaType, License license,
      String topic, Map<String, Object> connection) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");
    if (abbr == null)
      throw new NullPointerException("Parameter 'abbr' cannot be null.");
    if (abbr.length() == 0)
      throw new IllegalArgumentException("Parameter 'abbr' cannot be empty.");
    if (mediaType == null)
      throw new NullPointerException("Parameter 'mediaType' cannot be null.");
    if (license == null)
      throw new NullPointerException("Parameter 'license' cannot be null.");
    if (topic == null)
      throw new NullPointerException("Parameter 'topic' cannot be null.");
    if (topic.length() == 0)
      throw new IllegalArgumentException("Parameter 'topic' cannot be empty.");
    if (connection == null)
      throw new NullPointerException("Parameter 'connection' cannot be null.");

    this.id = id;
    this.name = name;
    this.mediaType = mediaType;
    this.mediaTypeId = mediaType.getId();
    this.license = license;
    this.licenseId = license.getId();
    this.topic = topic;
    this.connection = connection;
  }

  /**
   * Creates a new instance of a DataSource object, initializes with specified
   * parameters.
   * 
   * @param id         The primary key
   * @param name       The unique name
   * @param abbr       The unique abbreviation
   * @param mediaType  Foreign key to the media type
   * @param license    Foreign key to the license
   * @param topic      The Kafka topic
   * @param connection The connection information
   * @param version    Row version value
   */
  public DataSource(int id, String name, String abbr, MediaType mediaType, License license,
      String topic, Map<String, Object> connection, int version) {
    this(id, name, abbr, mediaType, license, topic, connection);
    this.setVersion(version);
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
   * @return String return the code
   */
  public String getCode() {
    return code;
  }

  /**
   * @param code the code to set
   */
  public void setCode(String code) {
    this.code = code;
  }

  /**
   * @return ZonedDateTime return the lastRanOn
   */
  public ZonedDateTime getLastRanOn() {
    return lastRanOn;
  }

  /**
   * @param lastRanOn the lastRanOn to set
   */
  public void setLastRanOn(ZonedDateTime lastRanOn) {
    this.lastRanOn = lastRanOn;
  }

  /**
   * @return String return the topic
   */
  public String getTopic() {
    return topic;
  }

  /**
   * @param topic the topic to set
   */
  public void setTopic(String topic) {
    this.topic = topic;
  }

  /**
   * @return MediaType return the media type
   */
  public MediaType getMediaType() {
    return mediaType;
  }

  /**
   * @param mediaType the media type to set
   */
  public void setMediaType(MediaType mediaType) {
    this.mediaType = mediaType;
  }

  /**
   * @return License return the license
   */
  public License getLicense() {
    return license;
  }

  /**
   * @param license the license to set
   */
  public void setLicense(License license) {
    this.license = license;
  }

  /**
   * @return int return the mediaTypeId
   */
  public int getMediaTypeId() {
    return mediaTypeId;
  }

  /**
   * @param mediaTypeId the mediaTypeId to set
   */
  public void setMediaTypeId(int mediaTypeId) {
    this.mediaTypeId = mediaTypeId;
  }

  /**
   * @return int return the licenseId
   */
  public int getLicenseId() {
    return licenseId;
  }

  /**
   * @param licenseId the licenseId to set
   */
  public void setLicenseId(int licenseId) {
    this.licenseId = licenseId;
  }

  /**
   * @return Map{String, Object} return the connection
   */
  public Map<String, Object> getConnection() {
    return connection;
  }

  /**
   * @param connection the connection to set
   */
  public void setConnection(Map<String, Object> connection) {
    this.connection = connection;
  }

  /**
   * @return List{DataSourceSchedule} return the dataSourceSchedules
   */
  public List<DataSourceSchedule> getDataSourceSchedules() {
    return dataSourceSchedules;
  }

  /**
   * @param dataSourceSchedules the dataSourceSchedules to set
   */
  public void setDataSourceSchedules(List<DataSourceSchedule> dataSourceSchedules) {
    this.dataSourceSchedules = dataSourceSchedules;
  }

  /**
   * @return Integer return the parentId
   */
  public Integer getParentId() {
    return parentId;
  }

  /**
   * @param parentId the parentId to set
   */
  public void setParentId(Integer parentId) {
    this.parentId = parentId;
  }

  /**
   * @return DataSource return the parent
   */
  public DataSource getParent() {
    return parent;
  }

  /**
   * @param parent the parent to set
   */
  public void setParent(DataSource parent) {
    this.parent = parent;
  }

}
