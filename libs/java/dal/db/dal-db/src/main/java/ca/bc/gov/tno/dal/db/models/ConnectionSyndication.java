package ca.bc.gov.tno.dal.db.models;

import java.util.HashMap;
import java.util.Map;

import ca.bc.gov.tno.dal.db.AuthenticationTypes;
import ca.bc.gov.tno.dal.db.SyndicationTypes;

/**
 * SyndicationSources class, defines a syndication connection information. This
 * is used as a way to serialize data source connection information.
 */
public class ConnectionSyndication {

  private String url;

  private SyndicationTypes syndicationType;

  private AuthenticationTypes authenticationType;

  private String token;

  private String username;

  private String password;

  /**
   * Creates a new instance of a ConnectionSyndication object.
   */
  public ConnectionSyndication() {

  }

  /**
   * Creates a new instance of a ConnectionSyndication object, initializes with
   * specified parameters.
   * 
   * @param url                The URL to the syndication source
   * @param syndicationType    The syndication type
   * @param authenticationType The authentication type
   */
  public ConnectionSyndication(String url, SyndicationTypes syndicationType, AuthenticationTypes authenticationType) {
    if (url == null)
      throw new NullPointerException("Parameter 'url' cannot be null.");
    if (url.length() == 0)
      throw new IllegalArgumentException("Parameter 'url' cannot be empty.");

    this.url = url;
    this.syndicationType = syndicationType;
    this.authenticationType = authenticationType;
  }

  /**
   * Convert object into serializable HashMap for the datasource connection
   * property.
   * 
   * @return new instance of a HashMap containing connection information
   */
  public Map<String, Object> toMap() {
    return new HashMap<String, Object>() {
      {
        put("url", url);
        put("syndicationType", syndicationType);
        put("authenticationType", authenticationType);
        put("token", token);
        put("username", username);
        put("password", password);
      }
    };
  }

  /**
   * @return String return the url
   */
  public String getUrl() {
    return url;
  }

  /**
   * @param url the url to set
   */
  public void setUrl(String url) {
    this.url = url;
  }

  /**
   * @return SyndicationTypes return the syndicationType
   */
  public SyndicationTypes getSyndicationType() {
    return syndicationType;
  }

  /**
   * @param syndicationType the syndicationType to set
   */
  public void setSyndicationType(SyndicationTypes syndicationType) {
    this.syndicationType = syndicationType;
  }

  /**
   * @return AuthenticationTypes return the authenticationType
   */
  public AuthenticationTypes getauthenticationType() {
    return authenticationType;
  }

  /**
   * @param authenticationType the authenticationType to set
   */
  public void setauthenticationType(AuthenticationTypes authenticationType) {
    this.authenticationType = authenticationType;
  }

  /**
   * @return String return the token
   */
  public String getToken() {
    return token;
  }

  /**
   * @param token the token to set
   */
  public void setToken(String token) {
    this.token = token;
  }

  /**
   * @return String return the username
   */
  public String getUsername() {
    return username;
  }

  /**
   * @param username the username to set
   */
  public void setUsername(String username) {
    this.username = username;
  }

  /**
   * @return String return the password
   */
  public String getPassword() {
    return password;
  }

  /**
   * @param password the password to set
   */
  public void setPassword(String password) {
    this.password = password;
  }

}
