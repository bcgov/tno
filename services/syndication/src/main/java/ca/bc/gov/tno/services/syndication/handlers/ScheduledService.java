package ca.bc.gov.tno.services.syndication.handlers;

import java.util.Calendar;
import java.util.Date;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.apache.logging.log4j.util.ProcessIdUtil;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;
import org.springframework.context.event.EventListener;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.dal.db.Months;
import ca.bc.gov.tno.dal.db.WeekDays;
import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceService;
import ca.bc.gov.tno.services.ServiceStatus;
import ca.bc.gov.tno.services.syndication.Global;
import ca.bc.gov.tno.services.syndication.config.SyndicationConfig;
import ca.bc.gov.tno.services.syndication.config.SyndicationConfigs;
import ca.bc.gov.tno.services.syndication.events.ErrorEvent;
import ca.bc.gov.tno.services.syndication.events.FetchEvent;
import ca.bc.gov.tno.services.syndication.events.ServicePauseEvent;
import ca.bc.gov.tno.services.syndication.events.ServiceResumeEvent;
import ca.bc.gov.tno.services.syndication.events.ServiceStartEvent;
import ca.bc.gov.tno.services.syndication.events.SourceCompleteEvent;

/**
 * ScheduledService class, provides a process that manages the scheduling of
 * ingestion. This will ensure the process runs according to the configured
 * schedule.
 */
@Async
@Component
public class ScheduledService implements ApplicationListener<ServiceStartEvent> {
  private static final Logger logger = LogManager.getLogger(ScheduledService.class);

  @Autowired
  private SyndicationConfigs syndConfigs;

  @Autowired
  private ApplicationEventPublisher applicationEventPublisher;

  @Autowired
  private Global global;

  @Autowired
  private IDataSourceService dataSourceService;

  private String pid;

  /**
   * Creates a new instance of a ScheduledService object.
   */
  public ScheduledService() {
    this.pid = ProcessIdUtil.getProcessId();
  }

  /**
   * Get the current process pid.
   * 
   * @return Pid of the current process.
   */
  public String getPid() {
    return pid;
  }

  /**
   * When a pause event occurs change the status.
   * 
   * @param event The event.
   */
  @EventListener
  public void handlePauseEvent(ServicePauseEvent event) {
    logger.info("Pause service requested");
    global.setStatus(ServiceStatus.pause);
  }

  /**
   * When a resume event occurs change the status.
   * 
   * @param event The event.
   */
  @EventListener
  public void handleResumeEvent(ServiceResumeEvent event) {
    logger.info("Resume service requested");
    global.setStatus(ServiceStatus.resume);
  }

  /**
   * When a done event occurs inform the scheduler to continue.
   * 
   * @param event The event.
   */
  @EventListener
  public void handleFeedDoneEvent(SourceCompleteEvent event) {
    var caller = (ScheduledService) event.getSource();
    if (caller.pid == this.pid) {
      logger.info("Feed completed processing");
      var config = event.getConfig();

      try {
        updateConfig(config, new Date(System.currentTimeMillis()));
        // Reset failures as the source has been completed 'successfully'.
        global.setFailedAttempts(0);
      } catch (Exception ex) {
        // If an error occurs we don't want to block the current process, but we do want
        // to log it.
        var errorEvent = new ErrorEvent(this, ex);
        applicationEventPublisher.publishEvent(errorEvent);
      }
      synchronized (this) {
        notify();
      }
    }
  }

  /**
   * When the start even occurs run the scheduler.
   * 
   * @param event The event.
   */
  @Override
  public void onApplicationEvent(ServiceStartEvent event) {
    logger.info("Syndication Ingest service start event received.");
    run();
  }

  /**
   * Based on the schedule keep checking for new content from the data source.
   */
  public void run() {
    try {
      // TODO: Handle an array of sources.
      // TODO: Fetch sources from database.

      // Fetch syndication data sources from TNO DB.
      if (syndConfigs.getSources().isEmpty()) {
        var dataSources = dataSourceService.findAll();
        dataSources.forEach(ds -> syndConfigs.getSources().add(new SyndicationConfig(ds)));
      }

      var index = 0;
      global.setStatus(ServiceStatus.running);

      while (global.getStatus() != ServiceStatus.sleeping) {
        if (global.getStatus() == ServiceStatus.pause) {
          global.setStatus(ServiceStatus.paused);
        } else if (global.getStatus() == ServiceStatus.resume) {
          global.setStatus(ServiceStatus.running);
        }

        if (global.getStatus() == ServiceStatus.running) {
          // Reset index to start over.
          if (index == syndConfigs.getSources().size())
            index = 0;

          var config = syndConfigs.getSources().get(index);
          // Make request to TNO DB for data source configuration settings.
          config = fetchConfig(config);

          // Determine if the data source should be imported based on the configured
          // schedule.
          if (verifySchedule(new Date(System.currentTimeMillis()), config)) {
            var fetchEvent = new FetchEvent(this, config);
            applicationEventPublisher.publishEvent(fetchEvent);

            // TODO: Remove synchronous limitations.
            // Wait until the SourceCompleteEvent is received.
            synchronized (this) {
              wait();
            }
          }

          index++;
        }

        // Probably not needed, but didn't want a run away process. Which can occur
        // during pause, or if all syndication sources are disabled.
        Thread.sleep(50);
      }
    } catch (InterruptedException ex) {
      global.setStatus(ServiceStatus.sleeping);
      var errorEvent = new ErrorEvent(this, ex);
      applicationEventPublisher.publishEvent(errorEvent);
    }
  }

  /**
   * Calculate the time to wait until the next day's runAt value.
   * 
   * @param config Configuration settings.
   * @return Number of milliseconds to wait before running again.
   */
  private long calcWait(SyndicationConfig config) {
    var runAt = config.getRunAt();

    if (runAt == null)
      return 0;

    var runAtCal = Calendar.getInstance();
    runAtCal.setTime(runAt);
    var runAtHour = runAtCal.get(Calendar.HOUR_OF_DAY);
    var runAtMinute = runAtCal.get(Calendar.MINUTE);
    runAtCal.add(Calendar.DATE, 1);
    runAtCal.set(Calendar.HOUR, runAtHour);
    runAtCal.set(Calendar.MINUTE, runAtMinute);
    runAtCal.set(Calendar.SECOND, 0);

    var nowCal = Calendar.getInstance();
    var now = new Date(System.currentTimeMillis());
    nowCal.setTime(now);

    return runAtCal.getTimeInMillis() - nowCal.getTimeInMillis();
  }

  /**
   * Determine if the schedule allows for the process to run at this point in
   * time.
   * 
   * @param now
   * @param config
   * @return Whether the process should be run.
   */
  private boolean verifySchedule(Date now, SyndicationConfig config) {
    var cal = Calendar.getInstance();
    cal.setTime(now);

    var isEnabled = config.isEnabled();
    var isRun = verifyDelay(cal, config);
    var isRunAt = verifyRunAt(cal, config);
    var isDayOfMonth = verifyDayOfMonth(cal, config);
    var isWeekDay = verifyWeekDay(cal, config);
    var isMonth = verifyMonth(cal, config);
    return isEnabled && isRun && isRunAt && isDayOfMonth && isWeekDay && isMonth;
  }

  /**
   * Determine if the scheduled delay has been exceeded.
   * 
   * @param now
   * @param config
   * @return
   */
  private boolean verifyDelay(Calendar now, SyndicationConfig config) {
    var delay = config.getDelay();
    var lastRanOn = config.getLastRanOn();

    if (delay == 0 || lastRanOn == null)
      return true;

    // Add the delay to the last ran on to determine if it should run again.
    var next = Calendar.getInstance();
    next.setTime(config.getLastRanOn());
    next.add(Calendar.MILLISECOND, delay);

    return next.before(now);
  }

  /**
   * Verify that the process can run at this time in the day.
   * 
   * @param now
   * @param config
   * @return Whether the process should be run.
   */
  private boolean verifyRunAt(Calendar now, SyndicationConfig config) {
    var runAt = config.getRunAt();

    // TODO: Make this work across multiple instances.
    // If the data source has been run its max repeat limit, don't run again.
    if (config.getRepeat() > 0 && config.getRanCounter() >= config.getRepeat()
        && (runAt == null || config.getLastRanOn().after(runAt)))
      return false;

    // No limitation imposed by runAt, so always run.
    if (runAt == null)
      return true;

    var runAtCal = Calendar.getInstance();
    runAtCal.setTime(runAt);

    // If runAt is in the future don't run.
    if (runAtCal.after(now))
      return false;

    // If runAt is in the past we are only interested in the time.
    var runAtHour = runAtCal.get(Calendar.HOUR_OF_DAY);
    var runAtMinute = runAtCal.get(Calendar.MINUTE);
    var hour = now.get(Calendar.HOUR_OF_DAY);
    var minute = now.get(Calendar.MINUTE);

    return (runAtHour < hour) || (runAtHour == hour && runAtMinute <= minute);
  }

  /**
   * Verify that the process can run on this day of the month.
   * 
   * @param now
   * @param config
   * @return Whether the process should be run.
   */
  private boolean verifyDayOfMonth(Calendar now, SyndicationConfig config) {
    var dayOfMonth = now.get(Calendar.DAY_OF_MONTH);
    var runOnDayOfMonth = config.getDayOfMonth();

    return runOnDayOfMonth == 0 || dayOfMonth == runOnDayOfMonth;
  }

  /**
   * Verify that the process can run on this day of the week.
   * 
   * @param now
   * @param config
   * @return Whether the process should be run.
   */
  private boolean verifyWeekDay(Calendar now, SyndicationConfig config) {
    var dayOfWeek = now.get(Calendar.DAY_OF_WEEK);
    var runOnWeekDays = config.getRunOnWeekDays();

    if (runOnWeekDays == null || runOnWeekDays.contains(WeekDays.NA))
      return true;

    switch (dayOfWeek) {
    case (1):
      return runOnWeekDays.contains(WeekDays.Monday);
    case (2):
      return runOnWeekDays.contains(WeekDays.Tuesday);
    case (3):
      return runOnWeekDays.contains(WeekDays.Wednesday);
    case (4):
      return runOnWeekDays.contains(WeekDays.Thursday);
    case (5):
      return runOnWeekDays.contains(WeekDays.Friday);
    case (6):
      return runOnWeekDays.contains(WeekDays.Saturday);
    case (7):
      return runOnWeekDays.contains(WeekDays.Sunday);
    }
    return false;
  }

  /**
   * Verify that the process can run on this month.
   * 
   * @param now
   * @param config
   * @return Whether the process should be run.
   */
  private boolean verifyMonth(Calendar now, SyndicationConfig config) {
    var month = now.get(Calendar.MONTH);
    var runOnMonth = config.getRunOnMonths();

    if (runOnMonth == null || runOnMonth.contains(Months.NA))
      return true;

    switch (month) {
    case (0):
      return runOnMonth.contains(Months.January);
    case (1):
      return runOnMonth.contains(Months.February);
    case (2):
      return runOnMonth.contains(Months.March);
    case (3):
      return runOnMonth.contains(Months.April);
    case (4):
      return runOnMonth.contains(Months.May);
    case (5):
      return runOnMonth.contains(Months.June);
    case (6):
      return runOnMonth.contains(Months.July);
    case (7):
      return runOnMonth.contains(Months.August);
    case (8):
      return runOnMonth.contains(Months.September);
    case (9):
      return runOnMonth.contains(Months.October);
    case (10):
      return runOnMonth.contains(Months.November);
    case (11):
      return runOnMonth.contains(Months.December);
    }
    return false;
  }

  /**
   * Update the data source with the current ranAt date and time.
   * 
   * @param config
   * @param ranAt
   */
  private void updateConfig(SyndicationConfig config, Date ranAt) {
    if (config == null)
      throw new IllegalArgumentException("Parameter 'config' is required.");

    if (config.getRepeat() > 0)
      config.setRanCounter(config.getRanCounter() + 1);
    config.setLastRanOn(ranAt);

    var result = dataSourceService.findByCode(config.getId());
    if (result.isPresent()) {
      var dataSource = result.get();
      dataSource.setLastRanOn(ranAt);
      dataSourceService.update(dataSource);
    }
  }

  /**
   * Make a request to the TNO DB to fetch an updated configuration. If none is
   * found, return the current config. Log if the config is different.
   */
  private SyndicationConfig fetchConfig(SyndicationConfig config) {
    if (config == null)
      throw new IllegalArgumentException("Parameter 'config' is required.");

    var result = dataSourceService.findByCode(config.getId());

    // If the database does not have a config for this source, then log warning.
    if (result.isEmpty()) {
      logger.warn(String.format("Data source '%s' does not exist in database", config.getId()));
      return config;
    }

    var newConfig = new SyndicationConfig(result.get());

    // TODO: Check for all conditions.
    if (config.isEnabled() != newConfig.isEnabled() || !config.getTopic().equals(newConfig.getTopic())
        || !config.getType().equals(newConfig.getType()) || config.getDelay() != newConfig.getDelay()
        || !config.getRunAt().equals(newConfig.getRunAt()) || !config.getUrl().equals(newConfig.getUrl())
        || config.getRepeat() != newConfig.getRepeat())
      logger.warn(String.format("Configuration has been changed for data source '%s'", config.getId()));

    return newConfig;
  }
}
