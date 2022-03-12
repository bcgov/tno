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
   * A unique short name to identify the data source.
   */
  @Column(name = "short_name", nullable = false)
  private String shortName = "";

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
   * Foreign key to the data location.
   */
  @Column(name = "data_location_id", nullable = false)
  private int dataLocationId;

  /**
   * The data location reference.
   */
  @JsonBackReference("dataLocation")
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "data_location_id", insertable = false, updatable = false)
  private DataLocation dataLocation;

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
   * Number of times service should attempt to connect and read from this data
   * source before giving up.
   */
  @Column(name = "retry_limit", nullable = false)
  private int retryLimit = 3;

  /**
   * Number of failed attempts to fetch or read data source.
   */
  @Column(name = "failed_attempts", nullable = false)
  private int failedAttempts = 0;

  /**
   * Whether this data source should be included in the CBRA report.
   */
  @Column(name = "in_cbra", nullable = false)
  private boolean inCBRA = false;

  /**
   * Whether this data source should be included in the analysis report.
   */
  @Column(name = "in_analysis", nullable = false)
  private boolean inAnalysis = false;

  /**
   * JSON configuration values for the ingestion services.
   */
  // TODO: Switch to JSON in DB. Hibernate has issues with JSON which I haven't
  // been able to solve.
  @Convert(converter = HashMapToStringConverter.class)
  @Column(name = "connection", nullable = false, columnDefinition = "text")
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
   * @param name       The unique name
   * @param code       The unique abbreviation
   * @param mediaType  Foreign key to the media type
   * @param location   Foreign key to the data location
   * @param license    Foreign key to the license
   * @param topic      The Kafka topic
   * @param connection The connection information
   */
  public DataSource(String name, String code, MediaType mediaType, DataLocation location,
      License license, String topic, Map<String, Object> connection) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");
    if (code == null)
      throw new NullPointerException("Parameter 'code' cannot be null.");
    if (code.length() == 0)
      throw new IllegalArgumentException("Parameter 'code' cannot be empty.");
    if (mediaType == null)
      throw new NullPointerException("Parameter 'mediaType' cannot be null.");
    if (license == null)
      throw new NullPointerException("Parameter 'license' cannot be null.");
    if (location == null)
      throw new NullPointerException("Parameter 'location' cannot be null.");
    if (topic == null)
      throw new NullPointerException("Parameter 'topic' cannot be null.");
    if (topic.length() == 0)
      throw new IllegalArgumentException("Parameter 'topic' cannot be empty.");
    if (connection == null)
      throw new NullPointerException("Parameter 'connection' cannot be null.");

    this.name = name;
    this.code = code;
    this.mediaType = mediaType;
    this.mediaTypeId = mediaType.getId();
    this.dataLocation = location;
    this.dataLocationId = location.getId();
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
   * @param code       The unique abbreviation
   * @param mediaType  Foreign key to the media type
   * @param location   Foreign key to the data location
   * @param license    Foreign key to the license
   * @param topic      The Kafka topic
   * @param connection The connection information
   */
  public DataSource(int id, String name, String code, MediaType mediaType, DataLocation location,
      License license, String topic, Map<String, Object> connection) {
    this(name, code, mediaType, location, license, topic, connection);

    this.id = id;
  }

  /**
   * Creates a new instance of a DataSource object, initializes with specified
   * parameters.
   *
   * @param id         The primary key
   * @param name       The unique name
   * @param code       The unique abbreviation
   * @param mediaType  Foreign key to the media type
   * @param location   Foreign key to the data location
   * @param license    Foreign key to the license
   * @param topic      The Kafka topic
   * @param connection The connection information
   * @param version    Row version value
   */
  public DataSource(int id, String name, String code, MediaType mediaType, DataLocation location,
      License license, String topic, Map<String, Object> connection, int version) {
    this(id, name, code, mediaType, location, license, topic, connection);
    this.setVersion(version);
  }

  /**
   * Creates a new instance of a DataSource object, initializes with specified
   * parameters.
   *
   * @param name        The unique name
   * @param code        The unique abbreviation
   * @param mediaTypeId Foreign key to the media type
   * @param locationId  Foreign key to the data location
   * @param licenseId   Foreign key to the license
   * @param topic       The Kafka topic
   * @param connection  The connection information
   */
  public DataSource(String name, String code, int mediaTypeId, int locationId,
      int licenseId, String topic, Map<String, Object> connection) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");
    if (code == null)
      throw new NullPointerException("Parameter 'code' cannot be null.");
    if (code.length() == 0)
      throw new IllegalArgumentException("Parameter 'code' cannot be empty.");
    if (topic == null)
      throw new NullPointerException("Parameter 'topic' cannot be null.");
    if (topic.length() == 0)
      throw new IllegalArgumentException("Parameter 'topic' cannot be empty.");
    if (connection == null)
      throw new NullPointerException("Parameter 'connection' cannot be null.");

    this.name = name;
    this.code = code;
    this.mediaTypeId = mediaTypeId;
    this.dataLocationId = locationId;
    this.licenseId = licenseId;
    this.topic = topic;
    this.connection = connection;
  }

  /**
   * Creates a new instance of a DataSource object, initializes with specified
   * parameters.
   *
   * @param id          The primary key
   * @param name        The unique name
   * @param code        The unique abbreviation
   * @param mediaTypeId Foreign key to the media type
   * @param locationId  Foreign key to the data location
   * @param licenseId   Foreign key to the license
   * @param topic       The Kafka topic
   * @param connection  The connection information
   */
  public DataSource(int id, String name, String code, int mediaTypeId, int locationId,
      int licenseId, String topic, Map<String, Object> connection) {
    this(name, code, mediaTypeId, locationId, licenseId, topic, connection);

    this.id = id;
  }

  /**
   * Creates a new instance of a DataSource object, initializes with specified
   * parameters.
   *
   * @param id          The primary key
   * @param name        The unique name
   * @param code        The unique abbreviation
   * @param mediaTypeId Foreign key to the media type
   * @param locationId  Foreign key to the data location
   * @param licenseId   Foreign key to the license
   * @param topic       The Kafka topic
   * @param connection  The connection information
   * @param version     Row version value
   */
  public DataSource(int id, String name, String code, int mediaTypeId, int locationId,
      int licenseId, String topic, Map<String, Object> connection, int version) {
    this(id, name, code, mediaTypeId, locationId, licenseId, topic, connection);
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
   * @return String return the shortName
   */
  public String getShortName() {
    return shortName;
  }

  /**
   * @param shortName the shortName to set
   */
  public void setShortName(String shortName) {
    this.shortName = shortName;
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
   * @return int return the dataLocationId
   */
  public int getDataLocationId() {
    return dataLocationId;
  }

  /**
   * @param dataLocationId the dataLocationId to set
   */
  public void setDataLocationId(int dataLocationId) {
    this.dataLocationId = dataLocationId;
  }

  /**
   * @return DataLocation return the data location
   */
  public DataLocation getDataLocation() {
    return dataLocation;
  }

  /**
   * @param dataLocation the data location to set
   */
  public void setDataLocation(DataLocation dataLocation) {
    this.dataLocation = dataLocation;
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

  /**
   * @return int return the retryLimit
   */
  public int getRetryLimit() {
    return retryLimit;
  }

  /**
   * @param retryLimit the retryLimit to set
   */
  public void setRetryLimit(int retryLimit) {
    this.retryLimit = retryLimit;
  }

  /**
   * @return int return the failedAttempts
   */
  public int getFailedAttempts() {
    return failedAttempts;
  }

  /**
   * @param failedAttempts the failedAttempts to set
   */
  public void setFailedAttempts(int failedAttempts) {
    this.failedAttempts = failedAttempts;
  }

  /**
   * @return boolean return the inCBRA
   */
  public boolean inCBRA() {
    return inCBRA;
  }

  /**
   * @param inCBRA the inCBRA to set
   */
  public void setInCBRA(boolean inCBRA) {
    this.inCBRA = inCBRA;
  }

  /**
   * @return boolean return the inAnalysis
   */
  public boolean inAnalysis() {
    return inAnalysis;
  }

  /**
   * @param inAnalysis the inAnalysis to set
   */
  public void setInAnalysis(boolean inAnalysis) {
    this.inAnalysis = inAnalysis;
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

}
