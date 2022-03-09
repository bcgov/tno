package ca.bc.gov.tno.services.audio.config;

import org.springframework.context.annotation.Configuration;

import ca.bc.gov.tno.dal.db.entities.DataSource;
import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceService;
import ca.bc.gov.tno.services.data.config.DataSourceConfig;

/**
 * Configuration settings for the default Audio Feed. These values will be
 * used if a connection to the database cannot be made.
 */
@Configuration
public class AudioConfig extends DataSourceConfig {

  /**
   * Directory in which capturedAudio is being stored
   */
  private String captureDir;

  /**
   * Clip command template
   */
  private String clipCmd;

  /**
   * Clip directory
   */
  private String clipDir;

  /**
   * Time at which the audio began streaming
   */
  private long streamStartTime;

  /**
   * Code (name) of the capture service linked to this clip service.
   */
  private String captureService;

  /**
   * Code (name) of this audio service
   */
  private String audioService;

  /**
   * Was the last clip command validated and extracted?
   */
  private boolean clipSuccess;

  /**
   * The timezone of the audio service
   */
  private String timeZone;


  /**
   * Data source from which child schedules will be retrieved
   */
  private IDataSourceService dataService;

  /**
   * Creates a new instance of a AudioConfig object.
   */
  public AudioConfig() {

  }

  /**
   * Creates a new instance of a AudioConfig object, initializes with
   * specified parameters.
   * 
   * @param dataSource
   */
  public AudioConfig(DataSource dataSource, final IDataSourceService dataSourceService) {
    super(dataSource);

    var connection = dataSource.getConnection();

    setDataService(dataSourceService);
    setAudioService((String) dataSource.getCode());

    // Get values from the parent data source
    var result = dataSourceService.findById(dataSource.getParentId());
    var dataRecord = result.get();

    connection = dataRecord.getConnection();
    setCaptureService(dataRecord.getCode());
    setStreamStartTime((long) connection.get("streamStart"));
    setCaptureDir((String) connection.get("captureDir"));
    setClipDir((String) connection.get("clipDir"));
    setClipCmd((String) connection.get("clipCmd"));
    setTimezone((String) connection.get("timeZone"));
  }

  /**
   * @return String return the capture directory
   */
  public String getCaptureDir() {
    return captureDir;
  }

  /**
   * @param url the capture directory to set
   */
  public void setCaptureDir(String dir) {
    this.captureDir = dir;
  }

  /**
   * @return String return the audio capture start time
   */
  public long getStreamStartTime() {
    return streamStartTime;
  }

  /**
   * @param url audio capture start time to set
   */
  public void setStreamStartTime(long startTime) {
    this.streamStartTime = startTime;
  }

  /**
   * @return String return the clip command template
   */
  public String getClipCmd() {
    return clipCmd;
  }

  /**
   * @param url the clip command template to set
   */
  public void setClipCmd(String cmd) {
    this.clipCmd = cmd;
  }

  /**
   * @return String return the clip directory
   */
  public String getClipDir() {
    return clipDir;
  }

  /**
   * @param url the clip directory to set
   */
  public void setClipDir(String dir) {
    this.clipDir = dir;
  }

  /**
   * @return String return the name of the parent service
   */
  public String getCaptureService() {
    return captureService;
  }

  /**
   * @param service The name of the parent service
   */
  public void setCaptureService(String service) {
    this.captureService = service;
  }

  /**
   * @return String return the data source service
   */
  public IDataSourceService getDataService() {
    return dataService;
  }

  /**
   * @param service The data source service to use
   */
  public void setDataService(IDataSourceService service) {
    this.dataService = service;
  }

  /**
   * @return String return the name of the audio service
   */
  public String getAudioService() {
    return this.audioService;
  }

  /**
   * @param service The the name of the audio service to use
   */
  public void setAudioService(String service) {
    this.audioService = service;
  }

  /**
   * @return String return the success of the last clip command
   */
  public boolean getClipSuccess() {
    return this.clipSuccess;
  }

  /**
   * @param success The success status of the last clip
   */
  public void setClipSuccess(boolean success) {
    this.clipSuccess = success;
  }

  /**
   * @return String return the timezone of the audio service
   */
  public String getTimezone() {
    return this.timeZone;
  }

  /**
   * @param timeZone The timezone to use
   */
  public void setTimezone(String timeZone) {
    this.timeZone = timeZone;
  }
}
