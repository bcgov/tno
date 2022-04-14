package ca.bc.gov.tno.services.models;

import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * TokenResponse class, provides a model that represents keycloak token
 * response.
 */
public class TokenResponse {
  /**
   * The access token value.
   */
  @JsonProperty("access_token")
  public String accessToken;

  /**
   * Seconds before expiring.
   */
  @JsonProperty("expires_in")
  public long expiresIn;

  /**
   * Seconds before expiring.
   */
  @JsonProperty("refresh_expires_in")
  public long refreshExpiresIn;

  /**
   * Token type.
   */
  @JsonProperty("token_type")
  public String tokenType;

  /**
   * Not before policy.
   */
  @JsonProperty("not-before-policy")
  public long notBeforePolicy;

  /**
   * Token scope.
   */
  @JsonProperty("scope")
  public String scope;
}
