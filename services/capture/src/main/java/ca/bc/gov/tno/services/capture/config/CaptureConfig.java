package ca.bc.gov.tno.services.capture.config;

import org.springframework.context.annotation.Configuration;

import ca.bc.gov.tno.dal.db.entities.DataSource;
import ca.bc.gov.tno.services.data.config.DataSourceConfig;

/**
 * Configuration settings for the default Audio Feed. These values will be
 * used if a connection to the database cannot be made.
 */
@Configuration
public class CaptureConfig extends DataSourceConfig {
  /**
   * URL to the audio streaming source
   */
  private String audioUrl;

  /**
   * Capture command template
   */
  private String captureCmd;

  /**
   * Directory in which to store captured audio
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
   * Time until the streamed file is stale
   */
  private long streamTimeout;

  /**
   * Indicates whether the stream started
   */
  private boolean captureSuccess;

  /**
   * Time at which the audio began streaming
   */
  private long streamStartTime;

  /**
   * Creates a new instance of a AudioConfig object.
   */
  public CaptureConfig() {

  }

  /**
   * Creates a new instance of a AudioConfig object, initializes with
   * specified parameters.
   * 
   * @param dataSource
   */
  public CaptureConfig(DataSource dataSource) {
    super(dataSource);

    var connection = dataSource.getConnection();

    setAudioUrl((String) connection.get("audioUrl"));
    setCaptureCmd((String) connection.get("captureCmd"));
    setCaptureDir((String) connection.get("captureDir"));
    setClipCmd((String) connection.get("clipCmd"));
    setClipDir((String) connection.get("clipDir"));
    setStreamTimeout((String) connection.get("streamTimeout"));
    setStreamStartTime((long) connection.get("streamStart"));
  }

  /**
   * @return String return the capture command
   */
  public String getCaptureCmd() {
    return captureCmd;
  }

  /**
   * @param url the capture command to set
   */
  public void setCaptureCmd(String cmd) {
    this.captureCmd = cmd;
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
   * @return String return the audio url
   */
  public String getAudioUrl() {
    return audioUrl;
  }

  /**
   * @param url the audio url to set
   */
  public void setAudioUrl(String url) {
    this.audioUrl = url;
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
   * @return String return the stream timeout
   */
  public long getStreamTimeout() {
    return streamTimeout;
  }

  /**
   * @param url the stream timeout to set
   */
  public void setStreamTimeout(String timeout) {
    this.streamTimeout = Long.parseLong(timeout);
  }

  /**
   * @return String return success status of the stream capture
   */
  public boolean getCaptureSuccess() {
    return captureSuccess;
  }

  /**
   * @param url the stream capture success to set
   */
  public void setCaptureSuccess(boolean success) {
    this.captureSuccess = success;
  }
}
