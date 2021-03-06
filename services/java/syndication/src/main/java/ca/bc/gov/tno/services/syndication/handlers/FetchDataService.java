package ca.bc.gov.tno.services.syndication.handlers;

import java.util.Arrays;

import org.apache.http.HttpException;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;
import org.springframework.core.io.Resource;
import org.springframework.http.HttpEntity;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpMethod;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.converter.HttpMessageConverter;
import org.springframework.http.converter.ResourceHttpMessageConverter;
import org.springframework.http.converter.feed.AtomFeedHttpMessageConverter;
import org.springframework.http.converter.feed.RssChannelHttpMessageConverter;
import org.springframework.web.client.RestTemplate;

import ca.bc.gov.tno.services.data.TnoApi;
import ca.bc.gov.tno.services.data.config.ScheduleConfig;
import ca.bc.gov.tno.services.data.events.TransactionBeginEvent;
import ca.bc.gov.tno.services.events.ErrorEvent;
import ca.bc.gov.tno.services.syndication.config.SyndicationConfig;
import ca.bc.gov.tno.services.syndication.events.ProducerSendEvent;

import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;

import com.rometools.rome.feed.synd.SyndFeed;
import com.rometools.rome.io.SyndFeedInput;
import com.rometools.rome.io.XmlReader;

/**
 * FetchDataService class, provides an event handler that will make requests for
 * data sources when a FetchEvent is fired.
 */
@Async
@Component
public class FetchDataService implements ApplicationListener<TransactionBeginEvent> {
  private static final Logger logger = LogManager.getLogger(FetchDataService.class);

  private final ApplicationEventPublisher eventPublisher;
  private final HttpHeaders headers;
  private final RestTemplate rest;

  private Object caller;
  private SyndicationConfig config;
  private ScheduleConfig schedule;
  private final TnoApi api;

  /**
   * Create a new instance of a FetchDataService object.
   * 
   * @param eventPublisher Application event publisher object.
   * @param api            DAL data source service object.
   */
  @Autowired
  public FetchDataService(final ApplicationEventPublisher eventPublisher, final TnoApi api) {
    this.eventPublisher = eventPublisher;
    this.api = api;

    this.headers = new HttpHeaders();
    this.headers.add("Accept", "*/*");
    this.rest = new RestTemplate();

    // Need to do this because source feeds are notorious for invalid Content-Type.
    var atom = new AtomFeedHttpMessageConverter();
    atom.setSupportedMediaTypes(Arrays
        .asList(new MediaType[] { MediaType.APPLICATION_ATOM_XML, MediaType.APPLICATION_XML, MediaType.TEXT_XML }));
    var rss = new RssChannelHttpMessageConverter();
    rss.setSupportedMediaTypes(Arrays
        .asList(new MediaType[] { MediaType.APPLICATION_RSS_XML, MediaType.APPLICATION_XML, MediaType.TEXT_XML }));
    var xml = new ResourceHttpMessageConverter();
    xml.setSupportedMediaTypes(Arrays
        .asList(new MediaType[] { MediaType.APPLICATION_ATOM_XML, MediaType.APPLICATION_RSS_XML,
            MediaType.APPLICATION_XML, MediaType.TEXT_XML }));
    rest.setMessageConverters(Arrays.asList(new HttpMessageConverter<?>[] { atom, rss, xml }));
  }

  /**
   * Depending on the data source type, make an HTTP request for the feed. When
   * response is successful, publish an event so that the Kafka Producer can push
   * the content to a topic.
   */
  @Override
  public void onApplicationEvent(TransactionBeginEvent event) {
    caller = event.getSource();

    try {
      config = (SyndicationConfig) event.getDataSource();
      schedule = event.getSchedule();

      var url = config.getUrl();
      logger.info(String.format("Syndication fetch request started - %s", url));

      var entity = new HttpEntity<Resource>(headers);
      var response = rest.exchange(url, HttpMethod.GET, entity, Resource.class);
      var status = response.getStatusCode();

      if (status.series() == HttpStatus.Series.SUCCESSFUL) {
        logger.info(String.format("Syndication fetch response - %s %s", url, status));
        var input = new SyndFeedInput();
        var reader = new XmlReader(response.getBody().getInputStream());
        SyndFeed feed = input.build(reader);
        var readyEvent = new ProducerSendEvent(caller, config, schedule, feed);
        eventPublisher.publishEvent(readyEvent);

      } else if (status.series() == HttpStatus.Series.SERVER_ERROR
          || status.series() == HttpStatus.Series.CLIENT_ERROR) {
        var errorEvent = new ErrorEvent(caller, config,
            new HttpException(String.format("Syndication fetch response - %s %s", url, status)));
        eventPublisher.publishEvent(errorEvent);
      }

    } catch (Exception ex) {
      updateDataSource();
      var errorEvent = new ErrorEvent(caller, config, ex);
      eventPublisher.publishEvent(errorEvent);
    }
  }

  // TODO: Refactor to be part of the shared service package so that all services
  // inherit functionality.
  private void updateDataSource() {
    try {
      var dataSource = api.getDataSource(this.config.getId());
      var failedAttempts = dataSource.getFailedAttempts();
      dataSource.setFailedAttempts(++failedAttempts);
    } catch (Exception ex) {
      logger.error(String.format("Failed to update data-source '%s'", this.config.getId()), ex);
      // Ignore error but log it.
    }
  }
}
