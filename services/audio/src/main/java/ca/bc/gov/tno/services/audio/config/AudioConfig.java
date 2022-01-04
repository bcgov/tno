package ca.bc.gov.tno.services.audio.config;

import java.time.LocalTime;

import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.context.annotation.Configuration;

import ca.bc.gov.tno.dal.db.entities.DataSource;
import ca.bc.gov.tno.services.data.config.DataSourceConfig;

/**
 * Configuration settings for the default Audio Feed. These values will be
 * used if a connection to the database cannot be made.
 */
@Configuration
//@ConfigurationProperties("data")
public class AudioConfig extends DataSourceConfig {
  /**
   * URL to the audio data source feed
   */
  private String url;

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
   * Clip duration
   */
  private String clipDuration;

  /**
   * Clip directory
   */
  private String clipDir;

  /**
   * Time at which the audio began streaming
   */
  private long streamStartTime;

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
  public AudioConfig(DataSource dataSource) {
    super(dataSource);

    setUrl((String) dataSource.getConnection().get("url"));
  }

  /**
   * @return String return the url
   */
  public String getUrl() {
    return url;
  }

  /**
   * @param url the url to set
   */
  public void setUrl(String url) {
    this.url = url;
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
   * @return String return the clip duration
   */
  public String getClipDuration() {
    return clipDuration;
  }

  /**
   * @param url the clip duration to set
   */
  public void setClipDuration(String duration) {
    this.clipDuration = duration;
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


}
