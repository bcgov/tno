package ca.bc.gov.tno.models.home;

import java.io.Serializable;
import java.security.Principal;
import java.time.ZonedDateTime;
import java.util.ArrayList;
import java.util.Collection;

import org.keycloak.adapters.RefreshableKeycloakSecurityContext;
import org.keycloak.adapters.springsecurity.account.SimpleKeycloakAccount;
import org.keycloak.representations.AccessToken;
import org.springframework.security.core.Authentication;

public class PrincipalModel implements Serializable {
  private int id;
  private String uid;
  private String username;
  private String email;
  private String displayName;
  private String firstName;
  private String lastName;
  private ZonedDateTime lastLoginOn;
  private Collection<String> roles;
  private Collection<String> groups;

  public PrincipalModel() {
  }

  public PrincipalModel(Principal principal) {
    this.uid = principal.getName();
  }

  public PrincipalModel(Authentication authentication) {
    this.uid = authentication.getName();
    SimpleKeycloakAccount details = (SimpleKeycloakAccount) authentication.getDetails();
    this.roles = details.getRoles();
    this.groups = new ArrayList<String>();

    RefreshableKeycloakSecurityContext context = details.getKeycloakSecurityContext();
    AccessToken token = context.getToken();
    this.username = token.getPreferredUsername();
    this.email = token.getEmail();
    this.displayName = token.getNickName();
    this.firstName = token.getGivenName();
    this.lastName = token.getFamilyName();
  }

  /**
   * @return int return the id
   */
  public int getId() {
    return id;
  }

  /**
   * @param id the id to set
   */
  public void setId(int id) {
    this.id = id;
  }

  /**
   * @return String return the uid
   */
  public String getUid() {
    return uid;
  }

  /**
   * @param uid the uid to set
   */
  public void setUid(String uid) {
    this.uid = uid;
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
   * @return String return the email
   */
  public String getEmail() {
    return email;
  }

  /**
   * @param email the email to set
   */
  public void setEmail(String email) {
    this.email = email;
  }

  /**
   * @return String return the displayName
   */
  public String getDisplayName() {
    return displayName;
  }

  /**
   * @param displayName the displayName to set
   */
  public void setDisplayName(String displayName) {
    this.displayName = displayName;
  }

  /**
   * @return String return the firstName
   */
  public String getFirstName() {
    return firstName;
  }

  /**
   * @param firstName the firstName to set
   */
  public void setFirstName(String firstName) {
    this.firstName = firstName;
  }

  /**
   * @return String return the lastName
   */
  public String getLastName() {
    return lastName;
  }

  /**
   * @param lastName the lastName to set
   */
  public void setLastName(String lastName) {
    this.lastName = lastName;
  }

  /**
   * @return ZonedDateTime return the lastLoginOn
   */
  public ZonedDateTime getLastLoginOn() {
    return lastLoginOn;
  }

  /**
   * @param lastName the lastLoginOn to set
   */
  public void setLastLoginOn(ZonedDateTime lastLoginOn) {
    this.lastLoginOn = lastLoginOn;
  }

  /**
   * @return Collection{String} return the roles
   */
  public Collection<String> getRoles() {
    return roles;
  }

  /**
   * @param roles the roles to set
   */
  public void setRoles(Collection<String> roles) {
    this.roles = roles;
  }

  /**
   * @return Collection{String} return the groups
   */
  public Collection<String> getGroups() {
    return groups;
  }

  /**
   * @param groups the groups to set
   */
  public void setGroups(Collection<String> groups) {
    this.groups = groups;
  }

}
