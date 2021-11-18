package ca.bc.gov.tno.services.syndication.handlers;

import javax.annotation.PostConstruct;

import java.io.IOException;
import java.net.MalformedURLException;
import java.util.Arrays;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;
import org.springframework.http.HttpEntity;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpMethod;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.http.converter.HttpMessageConverter;
import org.springframework.http.converter.feed.AtomFeedHttpMessageConverter;
import org.springframework.http.converter.feed.RssChannelHttpMessageConverter;
import org.springframework.web.client.RestClientException;
import org.springframework.web.client.RestTemplate;

import ca.bc.gov.tno.services.syndication.config.SyndicationConfig;
import ca.bc.gov.tno.services.syndication.events.ErrorEvent;
import ca.bc.gov.tno.services.syndication.events.FeedReadyEvent;
import ca.bc.gov.tno.services.syndication.events.FetchEvent;

import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;

import com.rometools.rome.feed.atom.Feed;
import com.rometools.rome.feed.rss.Channel;
import com.rometools.rome.feed.synd.SyndFeedImpl;

/**
 * FetchHandler class, provides an event handler that will make requests for
 * data sources when a FetchEvent is fired.
 */
@Async
@Component
public class FetchHandler implements ApplicationListener<FetchEvent> {
  private static final Logger logger = LogManager.getLogger(FetchHandler.class);

  @Autowired
  private ApplicationEventPublisher applicationEventPublisher;

  private Object caller;
  private SyndicationConfig syndicationConfig;

  private HttpHeaders headers;
  private RestTemplate rest;

  /**
   * Create a new instance of a FetchEvent object.
   */
  public FetchHandler() {
  }

  /**
   * Initialize after constructor
   * 
   * @throws IOException
   */
  @PostConstruct
  public void init() throws MalformedURLException, IOException {
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
    rest.setMessageConverters(Arrays.asList(new HttpMessageConverter<?>[] { atom, rss }));
  }

  /**
   * Extract the event information and run the fetch process.
   */
  @Override
  public void onApplicationEvent(FetchEvent event) {
    caller = event.getSource();
    syndicationConfig = event.getConfig();
    run();
  }

  /**
   * Depending on the data source type, make an HTTP request for the feed. When
   * response is successful, publish an event so that the Kafka Producer can push
   * the content to a topic.
   */
  public void run() {
    try {
      var url = syndicationConfig.getUrl();
      logger.info(String.format("Syndication fetch request started - %s", url));

      var response = syndicationConfig.getType().equalsIgnoreCase("ATOM") ? runAtom(url) : runRss(url);
      var status = response.getStatusCode();

      if (status.series() == HttpStatus.Series.SUCCESSFUL) {
        logger.info(String.format("Syndication fetch response - %s %s", url, status));
        var readyEvent = new FeedReadyEvent(caller, syndicationConfig, new SyndFeedImpl(response.getBody()));
        applicationEventPublisher.publishEvent(readyEvent);

      } else if (status.series() == HttpStatus.Series.SERVER_ERROR
          || status.series() == HttpStatus.Series.CLIENT_ERROR) {
        logger.error(String.format("Syndication fetch response - %s %s", url, status));
      }

    } catch (RestClientException ex) {
      var errorEvent = new ErrorEvent(this, ex);
      applicationEventPublisher.publishEvent(errorEvent);
    }
  }

  /**
   * Fetch the ATOM feed.
   */
  private ResponseEntity<Feed> runAtom(String url) {
    var requestEntity = new HttpEntity<Feed>(headers);
    return rest.exchange(url, HttpMethod.GET, requestEntity, Feed.class);
  }

  /**
   * Fetch the RSS feed.
   */
  private ResponseEntity<Channel> runRss(String url) {
    var requestEntity = new HttpEntity<Channel>(headers);
    return rest.exchange(url, HttpMethod.GET, requestEntity, Channel.class);
  }
}
