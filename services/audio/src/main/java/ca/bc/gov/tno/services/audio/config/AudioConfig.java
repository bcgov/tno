package ca.bc.gov.tno.services.audio.config;

import org.springframework.context.annotation.Configuration;

import ca.bc.gov.tno.services.models.DataSource;
import ca.bc.gov.tno.services.data.TnoApi;
import ca.bc.gov.tno.services.data.config.DataSourceConfig;
import ca.bc.gov.tno.services.data.ApiException;
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
   * The timezone of the audio service
   */
  private String timeZone;

  /**
   * Creates a new instance of a AudioConfig object.
   */
  public AudioConfig() {

  }

  /**
   * Creates a new instance of a AudioConfig object, initializes with
   * specified parameters.
   * 
   * @param dataSource The data source for this configuration
   * @param api The api based data service
   */
  public AudioConfig(final DataSource dataSource, final TnoApi api) throws ApiException {
    super(dataSource);

    setAudioService((String) dataSource.getCode());

      if (dataSource.getParentId() != null) {
        var thisDs = api.getDataSource(dataSource.getCode());
        var parent = thisDs.getParent();
        var connection = parent.getConnection();
        setCaptureService(parent.getCode());
        setStreamStartTime((long) connection.get("streamStart"));
        setCaptureDir((String) connection.get("captureDir"));
        setClipDir((String) connection.get("clipDir"));
        setClipCmd((String) connection.get("clipCmd"));
        setTimezone((String) connection.get("timeZone"));
      }
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

  /**
   * @return String return the timeZone
   */
  public String getTimeZone() {
    return timeZone;
  }

  /**
   * @param timeZone the timeZone to set
   */
  public void setTimeZone(String timeZone) {
    this.timeZone = timeZone;
  }

}
