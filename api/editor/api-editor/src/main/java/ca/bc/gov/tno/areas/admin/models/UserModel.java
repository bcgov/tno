package ca.bc.gov.tno.areas.admin.models;

import java.time.ZonedDateTime;
import java.util.UUID;

import ca.bc.gov.tno.dal.db.entities.User;
import ca.bc.gov.tno.models.AuditColumnModel;

public class UserModel extends AuditColumnModel {

  /**
   * Primary key to identify the user.
   */
  private int id;

  /**
   * A unique name to identify the user account.
   */
  private String username;

  /**
   * A unique key to identify the user account.
   */
  private UUID key;

  /**
   * The user's email address
   */
  private String email;

  /**
   * The user's display name.
   */
  private String displayName = "";

  /**
   * The user's first name.
   */
  private String firstName = "";

  /**
   * The user's last name.
   */
  private String lastName = "";

  /**
   * Whether the user is enabled or disabled.
   */
  private boolean enabled = true;

  /**
   * Whether the email has been verified.
   */
  private boolean emailVerified;

  /**
   * The date and time the user last logged in.
   */
  private ZonedDateTime lastLoginOn;

  /**
   * A collection of user roles that belong to this role.
   */
  // private List<UserRole> userRoles = new ArrayList<>();

  public UserModel() {
  }

  public UserModel(User entity) {
    super(entity);

    if (entity != null) {
      this.id = entity.getId();
      this.key = entity.getKey();
      this.username = entity.getUsername();
      this.email = entity.getEmail();
      this.displayName = entity.getDisplayName();
      this.firstName = entity.getFirstName();
      this.lastName = entity.getLastName();
      this.enabled = entity.isEnabled();
      this.emailVerified = entity.isEmailVerified();
      this.lastLoginOn = entity.getLastLoginOn();
    }
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
   * @return UUID return the key
   */
  public UUID getKey() {
    return key;
  }

  /**
   * @param key the key to set
   */
  public void setKey(UUID key) {
    this.key = key;
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
   * @return boolean return the enabled
   */
  public boolean isEnabled() {
    return enabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setEnabled(boolean enabled) {
    this.enabled = enabled;
  }

  /**
   * @return boolean return the emailVerified
   */
  public boolean isEmailVerified() {
    return emailVerified;
  }

  /**
   * @param emailVerified the emailVerified to set
   */
  public void setEmailVerified(boolean emailVerified) {
    this.emailVerified = emailVerified;
  }

  /**
   * @return ZonedDateTime return the lastLoginOn
   */
  public ZonedDateTime getLastLoginOn() {
    return lastLoginOn;
  }

  /**
   * @param lastLoginOn the lastLoginOn to set
   */
  public void setLastLoginOn(ZonedDateTime lastLoginOn) {
    this.lastLoginOn = lastLoginOn;
  }

}
