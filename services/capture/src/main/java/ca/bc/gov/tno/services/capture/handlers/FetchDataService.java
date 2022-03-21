package ca.bc.gov.tno.services.capture.handlers;

import java.io.File;
import java.io.IOException;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;

import ca.bc.gov.tno.services.data.ScheduleHelper;
import ca.bc.gov.tno.services.data.config.ScheduleConfig;
import ca.bc.gov.tno.services.data.events.TransactionBeginEvent;
import ca.bc.gov.tno.services.data.events.TransactionCompleteEvent;
import ca.bc.gov.tno.services.events.ErrorEvent;
import ca.bc.gov.tno.services.capture.config.CaptureConfig;

import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;

/**
 * FetchDataService class, provides an event handler that will make requests for
 * data sources when a FetchEvent is fired.
 */
@Async
@Component
public class FetchDataService implements ApplicationListener<TransactionBeginEvent> {
  private static final Logger logger = LogManager.getLogger(FetchDataService.class);

  private final ApplicationEventPublisher eventPublisher;

  private Object caller;
  private CaptureConfig captureConfig;
  private ScheduleConfig schedule;
  /**
   * Create a new instance of a FetchDataService object.
   */
  @Autowired
  public FetchDataService(final ApplicationEventPublisher eventPublisher) {
    this.eventPublisher = eventPublisher;
  }

  /**
   * Indicates whether the current event is a stop event. This is true if the schedule's stop_at is
   * less than the current time.
   * 
   * @param schedule The schedule for the current event
   * @return true if this is a stop event, false otherwise
   */
  private boolean isaStopEvent(ScheduleConfig schedule) {

    var stopTime = ScheduleHelper.getMsDateTime(schedule.getStopAt(), captureConfig.getTimezone());
    var now = System.currentTimeMillis();

    return stopTime < now;
  }

  /**
   * Indicates whether the current event is a start event. This is true if the schedule's start_at is
   * less than the current time and greater than the stop time.
   * 
   * @param schedule The schedule for the current event
   * @return true if this is a start event, false otherwise
   */
  private boolean isaStartEvent(ScheduleConfig schedule) {

    var startTime = ScheduleHelper.getMsDateTime(schedule.getStartAt(), captureConfig.getTimezone());
    var stopTime = ScheduleHelper.getMsDateTime(schedule.getStopAt(), captureConfig.getTimezone());
    var now = System.currentTimeMillis();

    return startTime < now && stopTime > now;
  }

  /**
   * Start a media stream, if not already in progress.
   */
  @Override
  public void onApplicationEvent(TransactionBeginEvent event) {

    logger.info("Transaction begin event - start processing.");
    captureConfig = (CaptureConfig) event.getDataSource();
    String mediaSource = captureConfig.getCaptureDir() + "/" + captureConfig.getId() + ".mpg";

    try {
      caller = event.getSource();
      captureConfig = (CaptureConfig) event.getDataSource();
      schedule = event.getSchedule();

      if (isaStopEvent(schedule)) {
        stopMediaStream(captureConfig.getRunningCommand());
      } else 
      if (isaStartEvent(schedule)) { 
        boolean success = startMediaStream(mediaSource);
        captureConfig.setCaptureSuccess(success);
      } else {
        logger.info(captureConfig.getId() + ": Streaming has not been started yet.");
      }
      
      var doneEvent = new TransactionCompleteEvent(event.getSource(), captureConfig, schedule);
      eventPublisher.publishEvent(doneEvent);
    } catch (Exception ex) {
      var errorEvent = new ErrorEvent(this, ex);
      eventPublisher.publishEvent(errorEvent);
    }
  }

  /**
   * Obtain the location of the streaming file for this event and check that it is being written to.
   * If the file doesn't exist, or the media stream has dropped, start streaming from the media source.
   */
  private boolean startMediaStream(String mediaSource) {

    File captureFile = new File(mediaSource);
    String cmd = captureConfig.getCaptureCmd();
    boolean captureSuccess = false;
    long modified = 0;
    long current = 0;

    if (captureFile.exists()) {
      modified = captureFile.lastModified();
      current = System.currentTimeMillis();
    }

    // Start recording the media stream if it has dropped or the capture file doesn't exist
    if (modified == 0 || current - modified > captureConfig.getStreamTimeout()) {
      executeCaptureCmd(cmd, mediaSource);
      captureConfig.setStreamStartTime(System.currentTimeMillis());
      captureSuccess = true;
      logger.info(captureConfig.getId() + ": Started streaming.");
    } else {
      logger.info(captureConfig.getId() + ": Streaming currently in progress.");
    }

    return captureSuccess;
  }

  /**
   * Stop the streaming process that corresponds with the command <code>command</code>
   * 
   * @param runningNow The capture command currently running
   */
  private void stopMediaStream(String runningNow) {

    try {
      if (!runningNow.isEmpty()) {
        String[] cmdArray = new String[] {"pkill", "-f", runningNow};

        Runtime rt = Runtime.getRuntime();
        rt.exec(cmdArray);
        captureConfig.setRunningNow("");
        logger.info(captureConfig.getId() + ": Stopping streaming process.");
      } else {
        logger.info(captureConfig.getId() + ": Streaming has been terminated for today.");
      }
    } catch (IOException ex) {
      logger.error("Unable to stop the media stream using: pkill -f " + runningNow);
    }
  }

  /**
   * Executes a media stream capture command as defined in the captureConfig object
   * 
   * @param cmd The Linux command to execute (with substitution variables)
   * @param captureFilePath The full path of the output directory
   */
  private void executeCaptureCmd(String cmd, String captureFilePath) {

    if (!cmd.equals("")) {
      cmd = cmd.replace("[capture-url]", captureConfig.getAudioUrl());
      cmd = cmd.replace("[capture-path]", captureFilePath);

      String[] cmdArray = {
        "/bin/sh",
            "-c",
        cmd
      };

      try {
        Process p = new ProcessBuilder(cmdArray).start();
        captureConfig.setRunningNow(cmd.substring(0, cmd.indexOf(">") - 1));
        logger.info("Capture command executed '" + cmd);
      } catch (Exception e) {
        logger.info("Exception launching capture command '" + cmd + "': '" + e.getMessage() + "'", e);
      }
    }
  }
}