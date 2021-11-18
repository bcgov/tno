package ca.bc.gov.tno.services.config;

import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.context.event.ApplicationEventMulticaster;
import org.springframework.context.event.SimpleApplicationEventMulticaster;
import org.springframework.core.task.SimpleAsyncTaskExecutor;

/**
 * AsynchronousSpringEventsConfig class, configures application event
 * asynchronous publisher.
 */
@Configuration
public class AsynchronousSpringEventsConfig {
  /**
   * Provides an asynchronous event publisher.
   * 
   * @return A new instance of a ApplicationEventMulticaster object.
   */
  @Bean(name = "applicationEventMulticaster")
  public ApplicationEventMulticaster simpleApplicationEventMulticaster() {
    SimpleApplicationEventMulticaster eventMulticaster = new SimpleApplicationEventMulticaster();

    eventMulticaster.setTaskExecutor(new SimpleAsyncTaskExecutor());
    return eventMulticaster;
  }
}
