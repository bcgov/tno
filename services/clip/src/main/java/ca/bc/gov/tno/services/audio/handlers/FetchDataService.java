package ca.bc.gov.tno.services.audio.handlers;

import java.time.LocalDateTime;
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
public class FetchDataService implements ApplicationListener<TransactionBeginEvent> {
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
   * Calculate the length of the clip as the number of milliseconds between the
   * schedule's
   * start time and stop time.
   * 
   * @param schedule The schedule triggered for this execution cycle
   * @return The length of the clip to be extracted
   */
  private long getClipLength(ScheduleConfig schedule) {

    var startTime = ScheduleHelper.getMsDateTime(schedule.getStartAt(), audioConfig.getTimezone());
    var stopTime = ScheduleHelper.getMsDateTime(schedule.getStopAt(), audioConfig.getTimezone());
    var length = (stopTime - startTime) / 1000;

    return length;
  }

  /**
   * Get the offset of the clip in the captured file in seconds based on its start
   * and stop time. Clips never cross date
   * boundaries so no handling is required to wrap from one day to another.
   * 
   * @param schedule Schedule config being processed
   * @return The offset of the clip in the captured audio stream
   */
  private long getClipOffset(ScheduleConfig schedule) {

    var streamStart = audioConfig.getStreamStartTime();
    var startTime = ScheduleHelper.getMsDateTime(schedule.getStartAt(), audioConfig.getTimezone());
    var stopTime = ScheduleHelper.getMsDateTime(schedule.getStopAt(), audioConfig.getTimezone());
    var clipLength = getClipLength(schedule);
    var offset = 0L;

    if (stopTime - clipLength > streamStart) {
      offset = (startTime - streamStart) / 1000;
    }

    return offset;
  }

  /**
   * Verifies, using the schedule's start time and the capture services' stream
   * start time, whether the audio to be
   * extracted from the captured file has been recorded.
   * 
   * @param schedule The current clip schedule
   * @return whether the schedule's start time is later than the stream start time
   */
  private boolean verifyClipSchedule(ScheduleConfig schedule) {

    return ScheduleHelper.getMsDateTime(schedule.getStartAt(), audioConfig.getTimezone()) > audioConfig
        .getStreamStartTime();
  }

  /**
   * Start an audio stream, if not already in progress, and extract an audio clip.
   * Publish an audio event so that the Kafka
   * Producer can push the content to a topic.
   */
  @Override
  public void onApplicationEvent(TransactionBeginEvent event) {
    caller = event.getSource();

    try {
      audioConfig = (AudioConfig) event.getDataSource();
      var schedule = event.getSchedule();
      var mediaSource = audioConfig.getCaptureDir() + "/" + audioConfig.getCaptureService() + ".mpg";
      var captureKey = audioConfig.getId() + "_" + String.valueOf(System.currentTimeMillis());
      var cmd = audioConfig.getClipCmd();

      var destination = clipPath(schedule);

      if (verifyClipSchedule(schedule)) {
        executeClipCmd(cmd, mediaSource, destination, schedule);
      } else {
        throw new IOException("No audio available for this clip.");
      }

      var readyEvent = new ProducerSendEvent(caller, audioConfig, schedule, captureKey);
      eventPublisher.publishEvent(readyEvent);
    } catch (Exception ex) {
      var errorEvent = new ErrorEvent(caller, audioConfig, ex);
      eventPublisher.publishEvent(errorEvent);
    }
  }

  /**
   * Executes an audio clip command as defined in the audioConfig object.
   * 
   * @param cmd         The Linux command to execute (with substitution markers in
   *                    square brackets)
   * @param mediaSource The streamed file from which the clip will be extracted
   * @param destination The full path of the clip file to be written
   * @throws Exception
   */
  private void executeClipCmd(String cmd, String mediaSource, String destination, ScheduleConfig schedule)
      throws Exception {

    if (!cmd.equals("")) {
      long duration = getClipLength(schedule);
      long offset = getClipOffset(schedule);

      cmd = cmd.replace("[capture]", mediaSource);
      cmd = cmd.replace("[clip]", destination);
      cmd = cmd.replace("[duration]", String.valueOf(duration));
      cmd = cmd.replace("[start]", String.valueOf(offset));

      String[] cmdArray = {
          "/bin/sh",
          "-c",
          cmd
      };

      new ProcessBuilder(cmdArray).start();
      logger.info("Clip command executed '" + cmd);
    }
  }

  /**
   * Create a full file path based on date and source for use in captureEvent().
   * Also create any directories in this path that do not exist
   * at the time of execution. The path does NOT include file extension.
   * 
   * @return The fully qualified clip path name.
   * @throws IOException
   */
  private String clipPath(ScheduleConfig schedule) throws IOException {
    String path;
    LocalDateTime now = LocalDateTime.now();
    String year = String.format("%02d", now.getYear());
    String month = String.format("%02d", now.getMonthValue());
    String day = String.format("%02d", now.getDayOfMonth());

    // directory
    path = audioConfig.getClipDir() + "/" + year + month + day + "/" + audioConfig.getId();

    // create directory if necessary
    File dirTarget = new File(path);
    if (!dirTarget.isDirectory()) {
      if (!(dirTarget.mkdirs())) {
        throw new IOException("Could not create directory " + path);
      }
    }

    path = path + "/" + schedule.getName() + ".mov";

    return path;
  }
}