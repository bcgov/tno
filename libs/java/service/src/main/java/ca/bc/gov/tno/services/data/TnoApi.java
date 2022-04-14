package ca.bc.gov.tno.services.data;

import java.util.Arrays;
import java.util.List;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpEntity;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpMethod;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.client.ClientHttpRequestFactory;
import org.springframework.http.converter.FormHttpMessageConverter;
import org.springframework.http.converter.HttpMessageConverter;
import org.springframework.http.converter.json.MappingJackson2HttpMessageConverter;
import org.springframework.stereotype.Service;
import org.springframework.util.LinkedMultiValueMap;
import org.springframework.util.MultiValueMap;
import org.springframework.web.client.ResourceAccessException;
import org.springframework.web.client.RestTemplate;
import org.springframework.web.client.UnknownContentTypeException;

import ca.bc.gov.tno.services.models.ContentReference;
import ca.bc.gov.tno.services.models.DataSource;
import ca.bc.gov.tno.services.data.config.ApiConfig;
import ca.bc.gov.tno.services.models.TokenResponse;
import ca.bc.gov.tno.services.net.JsonRestTemplate;

/**
 * TnoApi class, provides helper methods for the managing the datasource.
 */
@Service
public final class TnoApi {
  private static final Logger logger = LogManager.getLogger(TnoApi.class);

  /** Configuration settings for the api */
  protected final ApiConfig config;

  protected final ClientHttpRequestFactory clientFactory;

  /** Java Web Token response from keycloak */
  private TokenResponse token;

  /** API AJAX request client. */
  private final RestTemplate client;

  /**
   * Creates a new instance of a TnoApi object.
   * 
   * @param config        API configuration settings.
   * @param clientFactory API configuration settings.
   */
  @Autowired
  public TnoApi(ApiConfig config, ClientHttpRequestFactory clientFactory) {
    this.config = config;
    this.clientFactory = clientFactory;
    this.client = createRestTemplate();
  }

  /**
   * @return The current access token.
   */
  public TokenResponse getToken() {
    return this.token;
  }

  private RestTemplate createRestTemplate() {
    var xmlOrJson = new MappingJackson2HttpMessageConverter();
    xmlOrJson.setSupportedMediaTypes(Arrays
        .asList(new MediaType[] { MediaType.TEXT_XML, MediaType.APPLICATION_JSON, MediaType.APPLICATION_NDJSON }));
    var form = new FormHttpMessageConverter();
    return createRestTemplate(Arrays.asList(new HttpMessageConverter<?>[] { xmlOrJson, form }));
  }

  private RestTemplate createRestTemplate(List<HttpMessageConverter<?>> messageConverters) {
    var rest = new RestTemplate(messageConverters);
    return rest;
  }

  /**
   * Make AJAX request to keycloak to fetch access token.
   * 
   * @return The token response.
   * @throws ApiException If the AJAX request fails for some reason.
   */
  public TokenResponse requestToken() throws ApiException {
    var url = config.getAuthorityUrl() + "/realms/" + config.getRealm() + "/protocol/openid-connect/token";
    var headers = new HttpHeaders();
    headers.setAccept(Arrays.asList(MediaType.APPLICATION_JSON));
    headers.setContentType(MediaType.APPLICATION_FORM_URLENCODED);
    var map = new LinkedMultiValueMap<String, String>();
    map.add("audience", config.getClientId());
    map.add("grant_type", "client_credentials");
    map.add("client_id", config.getClientId());
    map.add("client_secret", config.getClientSecret());
    var entity = new HttpEntity<MultiValueMap<String, String>>(map, headers);
    var response = client.exchange(url, HttpMethod.POST, entity, TokenResponse.class);

    var status = response.getStatusCode();
    if (status.series() == HttpStatus.Series.SUCCESSFUL) {
      this.token = response.getBody();
      return this.token;
    } else {
      throw new ApiException(String.format("Failed to retrieve token: %s", status.getReasonPhrase()));
    }
  }

  /**
   * Make an AJAX request to the API.
   * 
   * @param <T>          The type of object.
   * @param url          The URL to the endpoint.
   * @param method       The HTTP method.
   * @param data         The request body data.
   * @param responseType The response type.
   * @return A new instance of an object of the specified type.
   * @throws ApiException            If an error occurred when communicating with
   *                                 the API.
   * @throws ResourceAccessException An error occurred while making an AJAX
   *                                 request.
   */
  public <T> T request(String url, HttpMethod method, T data, Class<T> responseType)
      throws ApiException, ResourceAccessException {
    var token = requestToken();
    var jsonClient = new JsonRestTemplate();
    var headers = new HttpHeaders();
    headers.setAccept(Arrays.asList(MediaType.APPLICATION_JSON));
    headers.setBearerAuth(token.accessToken);
    headers.setContentType(MediaType.APPLICATION_JSON);
    var entity = new HttpEntity<T>(data, headers);
    try {
      var response = jsonClient.exchange(url, method, entity, responseType);

      var status = response.getStatusCode();
      if (status.series() == HttpStatus.Series.SUCCESSFUL && status != HttpStatus.NO_CONTENT) {
        return response.getBody();
      } else if (status == HttpStatus.NO_CONTENT) {
        return null;
      } else {
        throw new ApiException(
            String.format("API request failed '%s' '%s': %s", method, url, status.getReasonPhrase()));
      }
    } catch (UnknownContentTypeException ex) {
      logger.debug(ex.getMessage(), ex);
      throw ex;
    }
  }

  /**
   * Make AJAX request to fetch the data-source specified for the unique 'code'.
   * 
   * @param code Unique code to identify data-source.
   * @return A new instance of a DataSource, or an exception if not found.
   * @throws ApiException If the AJAX request fails for some reason.
   */
  public DataSource getDataSource(String code) throws ApiException, ResourceAccessException {
    var url = config.getHostUrl() + "/api/services/data/sources/" + code;
    return request(url, HttpMethod.GET, null, DataSource.class);
  }

  /**
   * Make AJAX request to fetch an array of data-source for the specified media
   * type 'name'.
   * 
   * @param mediaTypeName Name of media type.
   * @return An array of DataSource.
   * @throws ApiException If the AJAX request fails for some reason.
   */
  public List<DataSource> getDataSourcesForMediaType(String mediaTypeName)
      throws ApiException, ResourceAccessException {
    var url = config.getHostUrl() + "/api/services/data/sources/for/media/type/" + mediaTypeName;
    return List.of(request(url, HttpMethod.GET, null, DataSource[].class));
  }

  /**
   * Make AJAX request to update the data-source.
   * 
   * @param dataSource The data-source to update.
   * @return The updated data-source.
   * @throws ApiException If the AJAX request fails for some reason.
   */
  public DataSource update(DataSource dataSource) throws ApiException, ResourceAccessException {
    var url = config.getHostUrl() + "/api/services/data/sources/" + dataSource.getId();
    return request(url, HttpMethod.PUT, dataSource, DataSource.class);
  }

  /**
   * Make AJAX request to fetch the content reference specified for the unique
   * 'source' and 'uid'.
   * 
   * @param source Unique source to identify content reference.
   * @param uid    Unique id to identify content reference.
   * @return A new instance of a ContentReference, or an exception if not found.
   * @throws ApiException If the AJAX request fails for some reason.
   */
  public ContentReference getContentReference(String source, String uid) throws ApiException, ResourceAccessException {
    var url = config.getHostUrl() + "/api/services/content/references/" + source + "?uid=" + uid;
    return request(url, HttpMethod.GET, null, ContentReference.class);
  }

  /**
   * Make AJAX request to add the content reference.
   * 
   * @param contentReference The content reference to add.
   * @return The add content reference.
   * @throws ApiException If the AJAX request fails for some reason.
   */
  public ContentReference add(ContentReference contentReference)
      throws ApiException, ResourceAccessException {
    var url = config.getHostUrl() + "/api/services/content/references/" + contentReference.getSource();
    return request(url, HttpMethod.POST, contentReference, ContentReference.class);
  }

  /**
   * Make AJAX request to update the content reference.
   * 
   * @param contentReference The content reference to update.
   * @return The updated content reference.
   * @throws ApiException If the AJAX request fails for some reason.
   */
  public ContentReference update(ContentReference contentReference)
      throws ApiException, ResourceAccessException {
    var url = config.getHostUrl() + "/api/services/content/references/" + contentReference.getSource();
    return request(url, HttpMethod.PUT, contentReference, ContentReference.class);
  }
}
