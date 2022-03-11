package ca.bc.gov.tno.services.capture.handlers;

import java.util.HashMap;
import java.util.Map;
import java.io.File;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;
import org.springframework.context.event.EventListener;

import ca.bc.gov.tno.services.data.config.ScheduleConfig;
import ca.bc.gov.tno.services.data.events.TransactionBeginEvent;
import ca.bc.gov.tno.services.events.ErrorEvent;
import ca.bc.gov.tno.services.capture.config.CaptureConfig;
import ca.bc.gov.tno.services.capture.events.ProducerSendEvent;

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
  private Map<String, Process> captureProcesses = new HashMap<>();
  private ScheduleConfig schedule;
  /**
   * Create a new instance of a FetchDataService object.
   */
  @Autowired
  public FetchDataService(final ApplicationEventPublisher eventPublisher) {
    this.eventPublisher = eventPublisher;
  }

  /**
   * Start a media stream, if not already in progress.
   */
  @Override
  @EventListener
  public void onApplicationEvent(TransactionBeginEvent event) {

    captureConfig = (CaptureConfig) event.getDataSource();
    String mediaSource = captureConfig.getCaptureDir() + "/" + captureConfig.getId() + ".mpg";
    String captureKey = captureConfig.getId() + "_" +  String.valueOf(System.currentTimeMillis());

    try {
      caller = event.getSource();
      captureConfig = (CaptureConfig) event.getDataSource();
      schedule = event.getSchedule();

      boolean success = startMediaStream(mediaSource);
      captureConfig.setCaptureSuccess(success);
      
      var readyEvent = new ProducerSendEvent(caller, captureConfig, schedule, captureKey);
      eventPublisher.publishEvent(readyEvent);
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
    }

    return captureSuccess;
  }

  /**
   * Executes an media stream capture command as defined in the captureConfig object
   * 
   * @param cmd The Linux command to execute 
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
        captureProcesses.put(captureConfig.getId(), p);
        logger.info("Capture command executed '" + cmd);
      } catch (Exception e) {
        logger.info("Exception launching capture command '" + cmd + "': '" + e.getMessage() + "'", e);
      }
    }
  }
}