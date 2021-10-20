package ca.bc.gov.tno.dal.db.entities;

import java.util.Date;

import javax.persistence.CascadeType;
import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.JoinColumn;
import javax.persistence.ManyToOne;
import javax.persistence.Table;

@Entity
@Table(name = "\"DataSources\"")
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

  @ManyToOne()
  @JoinColumn(name = "\"typeId\"")
  private DataSourceType type;

  @ManyToOne()
  @JoinColumn(name = "\"licenseId\"")
  private License license;

  @ManyToOne()
  @JoinColumn(name = "\"scheduleId\"")
  private Schedule schedule;

  @Column(name = "\"topic\"", nullable = false)
  private String topic;

  @Column(name = "\"lastRanOn\"")
  private Date lastRanOn;

  public DataSource() {

  }

  public DataSource(int id, String name, String abbr, DataSourceType type, License license, Schedule schedule,
      String topic) {
    this.id = id;
    this.name = name;
    this.type = type;
    this.license = license;
    this.schedule = schedule;
    this.topic = topic;
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

}
