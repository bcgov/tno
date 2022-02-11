package ca.bc.gov.tno.services.data.events;

import org.springframework.context.ApplicationEvent;
import org.springframework.scheduling.annotation.Async;

import ca.bc.gov.tno.services.data.config.DataSourceConfig;
import ca.bc.gov.tno.services.data.config.ScheduleConfig;

/**
 * TransactionBeginEvent class, provides an event to indicate the transaction
 * has begun.
 */
@Async
public class TransactionBeginEvent extends ApplicationEvent {

  /**
   * The data source configuration.
   */
  private DataSourceConfig dataSource;

  /**
   * The schedule configuration.
   */
  private ScheduleConfig schedule;

  /**
   * Creates a new instance of an TransactionBeginEvent, initializes with
   * specified parameters.
   * 
   * @param source     The source of the event.
   * @param dataSource Data source configuration
   * @param schedule   Schedule configuration
   */
  public TransactionBeginEvent(Object source, DataSourceConfig dataSource, ScheduleConfig schedule) {
    super(source);
    this.dataSource = dataSource;
    this.schedule = schedule;
  }

  /**
   * @return The data source.
   */
  public DataSourceConfig getDataSource() {
    return dataSource;
  }

  /**
   * @return The data source schedule.
   */
  public ScheduleConfig getSchedule() {
    return schedule;
  }
}
