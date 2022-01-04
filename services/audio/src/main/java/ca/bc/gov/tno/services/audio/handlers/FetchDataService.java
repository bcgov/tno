package ca.bc.gov.tno.services.audio.handlers;

import java.time.LocalDateTime;
import java.io.File;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;
import org.springframework.context.event.EventListener;
import ca.bc.gov.tno.services.data.events.TransactionBeginEvent;
import ca.bc.gov.tno.services.events.ErrorEvent;
import ca.bc.gov.tno.services.audio.config.AudioConfig;
import ca.bc.gov.tno.services.audio.events.ProducerSendEvent;

import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;

/**
 * FetchDataService class, provides an event handler that will make requests for
 * data sources when a FetchEvent is fired.
 */
@Async
@Component
public class FetchDataService implements ApplicationListener<TransactionBeginEvent<AudioConfig>> {
  private static final Logger logger = LogManager.getLogger(FetchDataService.class);

  private final ApplicationEventPublisher eventPublisher;

  private Object caller;
  private AudioConfig audioConfig;

  /**
   * Create a new instance of a FetchDataService object.
   */
  @Autowired
  public FetchDataService(final ApplicationEventPublisher eventPublisher) {
    this.eventPublisher = eventPublisher;
  }

  /**
   * Start an audio stream, if not already in progress, and extract an audio clip. Publish an audio event so that the Kafka 
   * Producer can push the content to a topic.
   */
  @Override
  @EventListener
  public void onApplicationEvent(TransactionBeginEvent<AudioConfig> event) {

    audioConfig = event.getConfig();
    String mediaSource = audioConfig.getCaptureDir() + "/" + audioConfig.getId() + ".mpg";
    String cmd = audioConfig.getClipCmd();
    String destination = clipPath();

    try {
      caller = event.getSource();
      var url = audioConfig.getAudioUrl();
      var readyEvent = new ProducerSendEvent(caller, audioConfig, destination);

      if (startAudioStream()) {
        eventPublisher.publishEvent(readyEvent);
      } else {
        executeClipCmd(cmd, mediaSource, destination);
        eventPublisher.publishEvent(readyEvent);
      }
    } catch (Exception ex) {
      var errorEvent = new ErrorEvent(this, ex);
      eventPublisher.publishEvent(errorEvent);
    }
  }

  /**
   * Obtain the location of the streaming file for this event and check that it is being written to.
   * If the file doesn't exist, or the audio stream has dropped, start streaming from the media source.
   */
  private boolean startAudioStream() {

    String captureFilePath = audioConfig.getCaptureDir() + "/" + audioConfig.getId() + ".mpg";
    File captureFile = new File(captureFilePath);
    String cmd = audioConfig.getCaptureCmd();
    long created = 0;
    boolean startedStream = false;

    // Start recording the audio stream if it doesn't exist
    if (!captureFile.exists()) {
        executeCaptureCmd(cmd, captureFilePath);
        audioConfig.setStreamStartTime(System.currentTimeMillis());
        startedStream = true;
    } else {
      // Check to see if the audio stream has dropped
      // TODO: handle rollover from one day to next
      long modified = captureFile.lastModified();
      long current = System.currentTimeMillis();
      if (current - modified > 60000) {
        executeCaptureCmd(cmd, captureFilePath);
        audioConfig.setStreamStartTime(System.currentTimeMillis());
        startedStream = true;
      } 
    }

    return startedStream;
  }

  /**
   * Executes an audio stream capture command as defined in the audioConfig object
   * 
   * @param cmd The Linux command to execute 
   * @param captureFilePath The full path of the output directory
   */
  private void executeCaptureCmd(String cmd, String captureFilePath) {

    if (!cmd.equals("")) {
      cmd = cmd.replace("[capture-url]", audioConfig.getAudioUrl());
      cmd = cmd.replace("[capture-path]", captureFilePath);

      String[] cmda = {
        "/bin/sh",
            "-c",
        cmd
      };

      try {
        Process p = new ProcessBuilder(cmda).start();
        logger.info("Capture command executed '" + cmd);
      } catch (Exception e) {
        logger.info("Exception launching capture command '" + cmd + "': '" + e.getMessage() + "'", e);
      }
    }
  }

  /**
   * Executes an audio clip command as defined in the audioConfig object.
   * 
   * @param cmd The Linux command to execute 
   * @param mediaSource The streamed file from which the clip will be extracted
   * @param destination The full path of the clip file to be written
   */
  private void executeClipCmd(String cmd, String mediaSource, String destination) {

    if (!cmd.equals("")) {
      long streamStart = audioConfig.getStreamStartTime();
      long now = System.currentTimeMillis();
      String offset = String.valueOf((now - streamStart - audioConfig.getDelay()) / 1000);

      cmd = cmd.replace("[capture]", mediaSource);
      cmd = cmd.replace("[clip]", destination);
      cmd = cmd.replace("[duration]", audioConfig.getClipDuration());
      cmd = cmd.replace("[start]", offset);

      String[] cmda = {
        "/bin/sh",
            "-c",
        cmd
      };

      try {
        Process p = new ProcessBuilder(cmda).start();
        logger.info("Clip command executed '" + cmd);
      } catch (Exception e) {
        logger.info("Exception launching capture command '" + cmd + "': '" + e.getMessage() + "'", e);
      }
    }
  }

	/**
	 *  Create a full file path based on date and source for use in captureEvent(). Also create any directories in this path that do not exist
	 *  at the time of execution. The path does NOT include file extension.
	 *  
	 * @return The fully qualified clip path name.
	 */
	private String clipPath() {
		String path;
    LocalDateTime now = LocalDateTime.now();
    String year = String.format("%02d", now.getYear());
    String month = String.format("%02d", now.getMonthValue());
    String day = String.format("%02d", now.getDayOfMonth());
    String hour = String.format("%02d", now.getHour());
    String minute = String.format("%02d", now.getMinute());
    String second = String.format("%02d", now.getSecond());

		// directory
		path = audioConfig.getClipDir() + "/" + year + month + day + "/" + audioConfig.getId();

		// create directory if neccessary
		File dirTarget = new File(path);
		if (!dirTarget.isDirectory()) {
			try {
				if (!(dirTarget.mkdirs())) {
					logger.warn("Could not create directory '" + path + "'");
				}
			} catch (Exception ex) {
				logger.error("Exception creating directory '" + path + "': '", ex);
			}
		}

		// file name
		path = path + "/" + audioConfig.getId();		
		path = path + "_" + year + month + day + "_" + hour + minute + second + ".mov";

		return path;
	}
}