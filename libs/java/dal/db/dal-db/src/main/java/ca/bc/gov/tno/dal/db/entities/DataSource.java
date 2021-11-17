package ca.bc.gov.tno.dal.db.entities;

import java.util.Date;
import java.util.HashMap;
import java.util.Map;

import javax.persistence.Column;
import javax.persistence.Convert;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.JoinColumn;
import javax.persistence.ManyToOne;
import javax.persistence.SequenceGenerator;
import javax.persistence.Table;
import javax.persistence.Temporal;
import javax.persistence.TemporalType;

import ca.bc.gov.tno.dal.db.AuditColumns;
import ca.bc.gov.tno.dal.db.converters.HashMapToStringConverter;

/**
 * DataSource class, defines a set of data, how often it is requested, the
 * length of time it will be stored, and the Kafka topic it will be stored in.
 */
@Entity
@Table(name = "\"DataSource\"")
public class DataSource extends AuditColumns {
  /**
   * Primary key to identify the data source.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_DataSource")
  @SequenceGenerator(name = "seq_DataSource", allocationSize = 1)
  @Column(name = "\"id\"", nullable = false)
  private int id;

  /**
   * A unique name to identify the data source.
   */
  @Column(name = "\"name\"", nullable = false)
  private String name;

  /**
   * A unique abbreviation to identify the data source. This is used in the
   * content reference table.
   */
  @Column(name = "\"code\"", nullable = false)
  private String code;

  /**
   * A description of the data source.
   */
  @Column(name = "\"description\"")
  private String description;

  /**
   * Whether this record is enabled or disabled.
   */
  @Column(name = "\"isEnabled\"", nullable = false)
  private boolean enabled;

  /**
   * Foreign key to the data source type.
   */
  @Column(name = "\"typeId\"", nullable = false)
  private int typeId;

  /**
   * The data source type reference.
   */
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "\"typeId\"", insertable = false, updatable = false)
  private DataSourceType type;

  /**
   * Foreign key to the license.
   */
  @Column(name = "\"licenseId\"", nullable = false)
  private int licenseId;

  /**
   * The license reference.
   */
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "\"licenseId\"", insertable = false, updatable = false)
  private License license;

  /**
   * Foreign key to the schedule.
   */
  @Column(name = "\"scheduleId\"", nullable = false)
  private int scheduleId;

  /**
   * The schedule reference.
   */
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "\"scheduleId\"", insertable = false, updatable = false)
  private Schedule schedule;

  /**
   * The Kafka topic that content will be pushed into.
   */
  @Column(name = "\"topic\"", nullable = false)
  private String topic;

  /**
   * The date and time this data source was successfully ingested on.
   */
  @Temporal(TemporalType.TIMESTAMP)
  @Column(name = "\"lastRanOn\"")
  private Date lastRanOn;

  /**
   * JSON configuration values for the ingestion services.
   */
  @Convert(converter = HashMapToStringConverter.class)
  @Column(name = "\"connection\"", nullable = false, columnDefinition = "text") // TODO: Switch to JSON in DB
  private Map<String, Object> connection = new HashMap<>();

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
   * @param type       Foreign key to the data source type
   * @param license    Foreign key to the license
   * @param schedule   Foreign key to the schedule
   * @param topic      The Kafka topic
   * @param connection The connection information
   */
  public DataSource(int id, String name, String abbr, DataSourceType type, License license, Schedule schedule,
      String topic, Map<String, Object> connection) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");
    if (abbr == null)
      throw new NullPointerException("Parameter 'abbr' cannot be null.");
    if (abbr.length() == 0)
      throw new IllegalArgumentException("Parameter 'abbr' cannot be empty.");
    if (type == null)
      throw new NullPointerException("Parameter 'type' cannot be null.");
    if (license == null)
      throw new NullPointerException("Parameter 'license' cannot be null.");
    if (schedule == null)
      throw new NullPointerException("Parameter 'schedule' cannot be null.");
    if (topic == null)
      throw new NullPointerException("Parameter 'topic' cannot be null.");
    if (topic.length() == 0)
      throw new IllegalArgumentException("Parameter 'topic' cannot be empty.");
    if (connection == null)
      throw new NullPointerException("Parameter 'connection' cannot be null.");

    this.id = id;
    this.name = name;
    this.type = type;
    this.typeId = type.getId();
    this.license = license;
    this.licenseId = license.getId();
    this.schedule = schedule;
    this.scheduleId = schedule.getId();
    this.topic = topic;
    this.connection = connection;
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
   * @return Date return the lastRanOn
   */
  public Date getLastRanOn() {
    return lastRanOn;
  }

  /**
   * @param lastRanOn the lastRanOn to set
   */
  public void setLastRanOn(Date lastRanOn) {
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
   * @return DataSourceType return the type
   */
  public DataSourceType getType() {
    return type;
  }

  /**
   * @param type the type to set
   */
  public void setType(DataSourceType type) {
    this.type = type;
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
   * @return Schedule return the schedule
   */
  public Schedule getSchedule() {
    return schedule;
  }

  /**
   * @param schedule the schedule to set
   */
  public void setSchedule(Schedule schedule) {
    this.schedule = schedule;
  }

  /**
   * @return int return the typeId
   */
  public int getTypeId() {
    return typeId;
  }

  /**
   * @param typeId the typeId to set
   */
  public void setTypeId(int typeId) {
    this.typeId = typeId;
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
   * @return int return the scheduleId
   */
  public int getScheduleId() {
    return scheduleId;
  }

  /**
   * @param scheduleId the scheduleId to set
   */
  public void setScheduleId(int scheduleId) {
    this.scheduleId = scheduleId;
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
}
