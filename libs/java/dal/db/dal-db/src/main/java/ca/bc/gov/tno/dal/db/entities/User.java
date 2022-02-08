package ca.bc.gov.tno.dal.db.entities;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Objects;
import java.util.UUID;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.OneToMany;
import javax.persistence.SequenceGenerator;
import javax.persistence.Table;
import javax.persistence.Temporal;
import javax.persistence.TemporalType;

import ca.bc.gov.tno.dal.db.AuditColumns;

/**
 * User class, provides an entity to manage users in the db.
 */
@Entity
@Table(name = "user", schema = "public")
public class User extends AuditColumns {
  /**
   * Primary key to identify the user.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_user")
  @SequenceGenerator(name = "seq_user", allocationSize = 1)
  @Column(name = "id", nullable = false)
  private int id;

  /**
   * A unique name to identify the user account.
   */
  @Column(name = "username", nullable = false)
  private String username;

  /**
   * A unique key to identify the user account.
   */
  @Column(name = "key", nullable = false)
  private UUID key;

  /**
   * The user's email address
   */
  @Column(name = "email", nullable = false)
  private String email;

  /**
   * The user's display name.
   */
  @Column(name = "display_name", nullable = true)
  private String displayName = "";

  /**
   * The user's first name.
   */
  @Column(name = "first_name", nullable = true)
  private String firstName = "";

  /**
   * The user's last name.
   */
  @Column(name = "last_name", nullable = true)
  private String lastName = "";

  /**
   * Whether the user is enabled or disabled.
   */
  @Column(name = "is_enabled", nullable = false)
  private boolean enabled = true;

  /**
   * Whether the email has been verified.
   */
  @Column(name = "email_verified", nullable = false)
  private boolean emailVerified;

  /**
   * The date and time the user last logged in.
   */
  @Temporal(TemporalType.TIMESTAMP)
  @Column(name = "last_login_on", nullable = false)
  private Date lastLoginOn;

  /**
   * A collection of user roles that belong to this role.
   */
  @OneToMany(mappedBy = "user", fetch = FetchType.LAZY)
  private List<UserRole> userRoles = new ArrayList<>();

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
   * @param id       The primary key.
   * @param username The unique username to identify the user.
   * @param email    The user's email address.
   */
  public User(int id, String username, String email) {
    super();
    this.id = id;
    this.username = username;
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
    this.id = id;
    this.username = username;
    this.key = key;
    this.email = email;
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
   * @return Date return the lastLoginOn
   */
  public Date getLastLoginOn() {
    return lastLoginOn;
  }

  /**
   * @param lastLoginOn the lastLoginOn to set
   */
  public void setLastLoginOn(Date lastLoginOn) {
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

}
