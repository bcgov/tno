package ca.bc.gov.tno.areas.editor.models;

import javax.persistence.Persistence;

import java.time.ZonedDateTime;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import ca.bc.gov.tno.dal.db.entities.DataSource;
import ca.bc.gov.tno.models.AuditColumnModel;

public class DataSourceModel extends AuditColumnModel {
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
  private String shortName;

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
   * Foreign key to the c.
   */
  private int dataLocationId;

  /**
   * The dataLocation reference.
   */
  private DataLocationModel dataLocation;

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
   * Whether this data source should be included in the CBRA report.
   */
  private boolean inCBRA = false;

  /**
   * Whether this data source should be included in the analysis report.
   */
  private boolean inAnalysis = false;

  /**
   * JSON configuration values for the ingestion services.
   */
  private Map<String, Object> connection = new HashMap<>();

  /**
   * An array of schedules associated with this data source.
   */
  private List<ScheduleModel> schedules = new ArrayList<>();

  public DataSourceModel() {
  }

  public DataSourceModel(DataSource entity) {
    super(entity);

    if (entity != null) {
      var putil = Persistence.getPersistenceUtil();

      this.id = entity.getId();
      this.name = entity.getName();
      this.shortName = entity.getShortName();
      this.description = entity.getDescription();
      this.enabled = entity.isEnabled();
      this.mediaTypeId = entity.getMediaTypeId();
      if (putil.isLoaded(entity, "mediaType"))
        this.mediaType = new MediaTypeModel(entity.getMediaType());
      this.dataLocationId = entity.getDataLocationId();
      if (putil.isLoaded(entity, "dataLocation"))
        this.dataLocation = new DataLocationModel(entity.getDataLocation());
      this.licenseId = entity.getLicenseId();
      if (putil.isLoaded(entity, "license"))
        this.license = new LicenseModel(entity.getLicense());
      this.topic = entity.getTopic();
      this.lastRanOn = entity.getLastRanOn();
      this.retryLimit = entity.getRetryLimit();
      this.failedAttempts = entity.getFailedAttempts();
      this.inCBRA = entity.inCBRA();
      this.inAnalysis = entity.inAnalysis();
      this.connection = entity.getConnection();
      if (putil.isLoaded(entity, "dataSourceSchedules"))
        this.schedules
            .addAll(entity.getDataSourceSchedules().stream()
                .filter((dss) -> putil.isLoaded(dss, "schedule") && dss.getSchedule() != null)
                .map((dss) -> new ScheduleModel(dss.getSchedule())).toList());
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
   * @return MediaTypeModel return the mediaType
   */
  public MediaTypeModel getMediaType() {
    return mediaType;
  }

  /**
   * @param mediaType the mediaType to set
   */
  public void setMediaType(MediaTypeModel mediaType) {
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
   * @return DataLocationModel return the dataLocation
   */
  public DataLocationModel getDataLocation() {
    return dataLocation;
  }

  /**
   * @param dataLocation the dataLocation to set
   */
  public void setDataLocation(DataLocationModel dataLocation) {
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
   * @return LicenseModel return the license
   */
  public LicenseModel getLicense() {
    return license;
  }

  /**
   * @param license the license to set
   */
  public void setLicense(LicenseModel license) {
    this.license = license;
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
   * @return ScheduleModel return the schedule
   */
  public ScheduleModel getSchedule() {
    return schedule;
  }

  /**
   * @param schedule the schedule to set
   */
  public void setSchedule(ScheduleModel schedule) {
    this.schedule = schedule;
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

  /**
   * @return List<ScheduleModel> return the schedules
   */
  public List<ScheduleModel> getSchedules() {
    return schedules;
  }

  /**
   * @param schedules the schedules to set
   */
  public void setSchedules(List<ScheduleModel> schedules) {
    this.schedules = schedules;
  }

}
