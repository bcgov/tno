package ca.bc.gov.tno.services.models;

import java.time.ZonedDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;
import java.util.UUID;

import com.fasterxml.jackson.annotation.JsonFormat;
import com.fasterxml.jackson.databind.annotation.JsonDeserialize;

import ca.bc.gov.tno.services.converters.Settings;
import ca.bc.gov.tno.services.converters.ZonedDateTimeDeserializer;

/**
 * User class, provides an entity to manage users in the db.
 */
public class User extends AuditColumns {
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
  private boolean isEnabled = true;

  /**
   * Whether the email has been verified.
   */
  private boolean emailVerified;

  /**
   * The date and time the user last logged in.
   */
  @JsonFormat(shape = JsonFormat.Shape.STRING, pattern = Settings.dateTimeFormat)
  @JsonDeserialize(using = ZonedDateTimeDeserializer.class)
  private ZonedDateTime lastLoginOn;

  /**
   * A collection of user roles that belong to this user.
   */
  private List<UserRole> userRoles = new ArrayList<>();

  /**
   * A collection of time tracking that belong to this user.
   */
  private List<TimeTracking> timeTrackings = new ArrayList<>();

  /**
   * Creates a new instance of a user object.
   */
  public User() {
    this.key = UUID.randomUUID();
  }

  /**
   * Creates a new instance of a user object, initializes with specified
   * parameters.
   *
   * @param username The unique username to identify the user.
   * @param email    The user's email address.
   */
  public User(String username, String email) {
    super();

    if (username == null)
      throw new NullPointerException("Parameter 'username' cannot be null.");
    if (username.length() == 0)
      throw new IllegalArgumentException("Parameter 'username' cannot be empty.");
    if (email == null)
      throw new NullPointerException("Parameter 'email' cannot be null.");
    if (email.length() == 0)
      throw new IllegalArgumentException("Parameter 'email' cannot be empty.");

    this.username = username;
    this.email = email;
  }

  /**
   * Creates a new instance of a user object, initializes with specified
   * parameters.
   *
   * @param id       The primary key.
   * @param username The unique username to identify the user.
   * @param email    The user's email address.
   */
  public User(int id, String username, String email) {
    this(username, email);
    this.id = id;
  }

  /**
   * Creates a new instance of a user object, initializes with specified
   * parameters.
   *
   * @param id       The primary key.
   * @param username The unique username to identify the user.
   * @param email    The user's email address.
   * @param version  Row version value
   */
  public User(int id, String username, String email, long version) {
    this(id, username, email);
    this.setVersion(version);
  }

  /**
   * Creates a new instance of a user object, initializes with specified
   * parameters.
   *
   * @param username The unique username to identify the user.
   * @param key      A unique key to identify the user.
   * @param email    The user's email address.
   */
  public User(String username, UUID key, String email) {
    if (username == null)
      throw new NullPointerException("Parameter 'username' cannot be null.");
    if (username.length() == 0)
      throw new IllegalArgumentException("Parameter 'username' cannot be empty.");

    if (key == null)
      throw new NullPointerException("Parameter 'key' cannot be null.");

    if (email == null)
      throw new NullPointerException("Parameter 'email' cannot be null.");
    if (email.length() == 0)
      throw new IllegalArgumentException("Parameter 'email' cannot be empty.");

    this.username = username;
    this.key = key;
    this.email = email;
  }

  /**
   * Creates a new instance of a user object, initializes with specified
   * parameters.
   *
   * @param id       The primary key.
   * @param username The unique username to identify the user.
   * @param key      A unique key to identify the user.
   * @param email    The user's email address.
   */
  public User(int id, String username, UUID key, String email) {
    this(username, key, email);
    this.id = id;
  }

  /**
   * Creates a new instance of a user object, initializes with specified
   * parameters.
   *
   * @param id       The primary key.
   * @param username The unique username to identify the user.
   * @param key      A unique key to identify the user.
   * @param email    The user's email address.
   * @param version  Row version value
   */
  public User(int id, String username, UUID key, String email, long version) {
    this(id, username, key, email);
    this.setVersion(version);
  }

  /**
   * @return int return the id
   */
  public int getId() {
    return id;
  }

  /**
   * @return int return the username
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
  public boolean getIsEnabled() {
    return isEnabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setIsEnabled(boolean enabled) {
    this.isEnabled = enabled;
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

  /**
   * @param id the id to set
   */
  public void setId(int id) {
    this.id = id;
  }

  /**
   * @return List{UserRole} return the userRoles
   */
  public List<UserRole> getUserRoles() {
    return userRoles;
  }

  @Override
  public int hashCode() {
    int hash = 7;
    hash = 79 * hash + Objects.hashCode(this.id);
    hash = 79 & hash + Objects.hashCode(this.username);
    return hash;
  }

  @Override
  public boolean equals(Object obj) {
    if (this == obj) {
      return true;
    }
    if (obj == null) {
      return false;
    }
    if (getClass() != obj.getClass()) {
      return false;
    }
    final User user = (User) obj;
    if (!Objects.equals(this.username, user.username)) {
      return false;
    }
    return Objects.equals(this.id, user.id);
  }

  @Override
  public String toString() {
    final StringBuilder sb = new StringBuilder("User{");
    sb.append("id=").append(id);
    sb.append(", username='").append(username).append("\'");
    sb.append("}");
    return sb.toString();
  }

  /**
   * @return boolean return the isEnabled
   */
  public boolean isIsEnabled() {
    return isEnabled;
  }

  /**
   * @param userRoles the userRoles to set
   */
  public void setUserRoles(List<UserRole> userRoles) {
    this.userRoles = userRoles;
  }

  /**
   * @return List{TimeTracking} return the timeTrackings
   */
  public List<TimeTracking> getTimeTrackings() {
    return timeTrackings;
  }

  /**
   * @param timeTrackings the timeTrackings to set
   */
  public void setTimeTrackings(List<TimeTracking> timeTrackings) {
    this.timeTrackings = timeTrackings;
  }

}
