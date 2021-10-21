package ca.bc.gov.tno.dal.db.entities;

import java.util.Date;
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
import javax.persistence.Table;

import ca.bc.gov.tno.dal.db.converters.HashMapConverter;

/**
 * DataSource class, defines a set of data, how often it is requested, the
 * length of time it will be stored, and the Kafka topic it will be stored in.
 */
@Entity
@Table(name = "\"DataSource\"")
public class DataSource extends Audit {
  @Id
  @GeneratedValue(strategy = GenerationType.AUTO)
  @Column(name = "\"id\"", nullable = false)
  private int id;

  @Column(name = "\"name\"", nullable = false)
  private String name;

  @Column(name = "\"abbr\"", nullable = false)
  private String abbr;

  @Column(name = "\"description\"")
  private String description;

  @Column(name = "\"isEnabled\"", nullable = false)
  private boolean isEnabled;

  @Column(name = "\"typeId\"", nullable = false)
  private int typeId;

  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "\"typeId\"", insertable = false, updatable = false)
  private DataSourceType type;

  @Column(name = "\"licenseId\"", nullable = false)
  private int licenseId;

  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "\"licenseId\"", insertable = false, updatable = false)
  private License license;

  @Column(name = "\"scheduleId\"", nullable = false)
  private int scheduleId;

  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "\"scheduleId\"", insertable = false, updatable = false)
  private Schedule schedule;

  @Column(name = "\"topic\"", nullable = false)
  private String topic;

  @Column(name = "\"lastRanOn\"")
  private Date lastRanOn;

  @Convert(converter = HashMapConverter.class)
  @Column(name = "\"connection\"", nullable = false, columnDefinition = "json")
  private Map<String, Object> connection;

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
   * @return boolean return the isEnabled
   */
  public boolean isIsEnabled() {
    return isEnabled;
  }

  /**
   * @param isEnabled the isEnabled to set
   */
  public void setIsEnabled(boolean isEnabled) {
    this.isEnabled = isEnabled;
  }

  /**
   * @return String return the abbr
   */
  public String getAbbr() {
    return abbr;
  }

  /**
   * @param abbr the abbr to set
   */
  public void setAbbr(String abbr) {
    this.abbr = abbr;
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
   * @return Map<String, Object> return the connection
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
