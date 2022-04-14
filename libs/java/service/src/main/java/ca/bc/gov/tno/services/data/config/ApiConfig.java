package ca.bc.gov.tno.services.data.config;

import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.context.annotation.Configuration;

/**
 * Configuration settings to connect to the api.
 */
@Configuration
@ConfigurationProperties("api")
public class ApiConfig {
  /**
   * URL to the API host.
   */
  private String hostUrl;

  /**
   * Keycloak authority host URL.
   */
  private String authorityUrl;

  /**
   * Keycloak realm name.
   */
  private String realm;

  /**
   * Keycloak client ID.
   */
  private String clientId;

  /**
   * Keycloak client secret.
   */
  private String clientSecret;

  /**
   * Creates a new instance of a ApiConfig object.
   */
  public ApiConfig() {

  }

  /**
   * @return String return the hostUrl
   */
  public String getHostUrl() {
    return hostUrl;
  }

  /**
   * @param hostUrl the hostUrl to set
   */
  public void setHostUrl(String hostUrl) {
    this.hostUrl = hostUrl;
  }

  /**
   * @return String return the authorityUrl
   */
  public String getAuthorityUrl() {
    return authorityUrl;
  }

  /**
   * @param authorityUrl the authorityUrl to set
   */
  public void setAuthorityUrl(String authorityUrl) {
    this.authorityUrl = authorityUrl;
  }

  /**
   * @return String return the clientId
   */
  public String getClientId() {
    return clientId;
  }

  /**
   * @param clientId the clientId to set
   */
  public void setClientId(String clientId) {
    this.clientId = clientId;
  }

  /**
   * @return String return the clientSecret
   */
  public String getClientSecret() {
    return clientSecret;
  }

  /**
   * @param clientSecret the clientSecret to set
   */
  public void setClientSecret(String clientSecret) {
    this.clientSecret = clientSecret;
  }

  /**
   * @return String return the realm
   */
  public String getRealm() {
    return realm;
  }

  /**
   * @param realm the realm to set
   */
  public void setRealm(String realm) {
    this.realm = realm;
  }

}
