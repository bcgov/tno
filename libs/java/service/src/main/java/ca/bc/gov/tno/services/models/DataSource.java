package ca.bc.gov.tno.services.models;

import java.time.ZonedDateTime;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import com.fasterxml.jackson.annotation.JsonFormat;
import com.fasterxml.jackson.databind.annotation.JsonDeserialize;

import ca.bc.gov.tno.services.converters.Settings;
import ca.bc.gov.tno.services.converters.ZonedDateTimeDeserializer;

/**
 * DataSource class, defines a set of data, how often it is requested, the
 * length of time it will be stored, and the Kafka topic it will be stored in.
 */
public class DataSource extends AuditColumns {
  /**
   * Primary key to identify the data source.
   */
  private int id;

  /**
   * A unique name to identify the data source.
   */
  private String name;

  /**
   * A unique short name to identify the data source.
   */
  private String shortName = "";

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
  private boolean isEnabled = true;

  /**
   * Foreign key to the media type.
   */
  private int mediaTypeId;

  /**
   * The media type reference.
   */
  private MediaType mediaType;

  /**
   * Foreign key to the data location.
   */
  private int dataLocationId;

  /**
   * The data location reference.
   */
  private DataLocation dataLocation;

  /**
   * Foreign key to the license.
   */
  private int licenseId;

  /**
   * The license reference.
   */
  private License license;

  /**
   * The Kafka topic that content will be pushed into.
   */
  private String topic;

  /**
   * The Kafka topic that content will be pushed into.
   */
  private String scheduleType;

  /**
   * Foreign key to the type of content in this data source.
   */
  private int contentTypeId;

  /**
   * Foreign key to the type of content in this data source.
   */
  private int ownerId;

  /**
   * The contentType reference.
   */
  ContentType contentType;

  /**
   * The date and time this data source was successfully ingested on.
   */
  @JsonFormat(shape = JsonFormat.Shape.STRING, pattern = Settings.dateTimeFormat)
  @JsonDeserialize(using = ZonedDateTimeDeserializer.class)
  private ZonedDateTime lastRanOn;

  /**
   * Number of times service should attempt to connect and read from this data
   * source before giving up.
   */
  private int retryLimit = 3;

  /**
   * Number of failed attempts to fetch or read data source.
   */
  private int failedAttempts = 0;

  /**
   * JSON configuration values for the ingestion services.
   */
  // TODO: Switch to JSON in DB. Hibernate has issues with JSON which I haven't
  // been able to solve.
  private Map<String, Object> connection = new HashMap<>();

  /**
   * Foreign key to parent data source.
   */
  private Integer parentId;

  /**
   * Reference to the parent data source.
   */
  private DataSource parent;

  /**
   * A collection of data source schedules linked to this data source.
   */
  private List<DataSourceAction> dataSourceActions = new ArrayList<>();

  /**
   * A collection of data source schedules linked to this data source.
   */
  private List<DataSourceMetric> dataSourceMetrics = new ArrayList<>();

  /**
   * A collection of data source schedules linked to this data source.
   */
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
      License license, String topic, Map<String, Object> connection, long version) {
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
      int licenseId, String topic, Map<String, Object> connection, long version) {
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
  public boolean getIsEnabled() {
    return isEnabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setIsEnabled(boolean enabled) {
    this.isEnabled = enabled;
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
   * @return List{DataSourceAction} return the dataSourceActions
   */
  public List<DataSourceAction> getDataSourceActions() {
    return dataSourceActions;
  }

  /**
   * @param dataSourceActions the dataSourceActions to set
   */
  public void setDataSourceActions(List<DataSourceAction> dataSourceActions) {
    this.dataSourceActions = dataSourceActions;
  }

  /**
   * @return List{DataSourceMetric} return the dataSourceMetrics
   */
  public List<DataSourceMetric> getDataSourceMetrics() {
    return dataSourceMetrics;
  }

  /**
   * @param dataSourceMetrics the dataSourceMetrics to set
   */
  public void setDataSourceMetrics(List<DataSourceMetric> dataSourceMetrics) {
    this.dataSourceMetrics = dataSourceMetrics;
  }

  /**
   * @return String return the schedule type
   */
  public String getScheduleType() {
    return scheduleType;
  }

  /**
   * @param scheduleType the scheduleType to set
   */
  public void setScheduleType(String scheduleType) {
    this.scheduleType = scheduleType;
  }

  /**
   * @return int return the content type id
   */
  public int getContentTypeId() {
    return contentTypeId;
  }

  /**
   * @param contentTypeId the content type to set
   */
  public void setContentTypeId(int contentTypeId) {
    this.contentTypeId = contentTypeId;
  }

  /**
   * @return int return the owner id
   */
  public int getOwnerId() {
    return ownerId;
  }

  /**
   * @param ownerId the owner id to set
   */
  public void setOwnerId(int ownerId) {
    this.ownerId = ownerId;
  }

  /**
   * @return int return the content type reference
   */
  public ContentType getContentType() {
    return contentType;
  }

  /**
   * @param contentType the content type reference to set
   */
  public void setContentTypeId(ContentType contentType) {
    this.contentType = contentType;
  }
}
