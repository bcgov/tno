package ca.bc.gov.tno.services.models;

import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

/**
 * Role class, provides a way to manage roles.
 */
public class Role extends AuditColumns {
  /**
   * Primary key to identify the role.
   */
  private int id;

  /**
   * A unique name to identify the role.
   */
  private String name;

  /**
   * A unique key to identify the role.
   */
  private UUID key;

  /**
   * A description of the role.
   */
  private String description = "";

  /**
   * Whether this record is enabled or disabled.
   */
  private boolean isEnabled = true;

  /**
   * A collection of user roles that belong to this role.
   */
  private List<UserRole> userRoles = new ArrayList<>();

  /**
   * A collection of role claims that belong to this role.
   */
  private List<RoleClaim> roleClaims = new ArrayList<>();

  /**
   * Creates a new instance of a Role object.
   */
  public Role() {

  }

  /**
   * Creates a new instance of a Role object, initializes with specified
   * parameters.
   *
   * @param name Unique name
   */
  public Role(String name) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");

    this.name = name;
  }

  /**
   * Creates a new instance of a Role object, initializes with specified
   * parameters.
   *
   * @param id   Primary key
   * @param name Unique name
   */
  public Role(int id, String name) {
    this(name);
    this.id = id;
  }

  /**
   * Creates a new instance of a Role object, initializes with specified
   * parameters.
   *
   * @param id      Primary key
   * @param name    Unique name
   * @param version Row version value
   */
  public Role(int id, String name, long version) {
    this(id, name);
    this.setVersion(version);
  }

  /**
   * @return int return the id
   */
  public int getId() {
    return id;
  }

  /**
   * @return String return the name
   */
  public String getName() {
    return name;
  }

  /**
   * @param name the name to set
   */
  public void setName(String name) {
    this.name = name;
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
   * @return String return the description
   */
  public String getDescription() {
    return description;
  }

  /**
   * @param description the description to set
   */
  public void setDescription(String description) {
    this.description = description;
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

  /**
   * @return List{RoleClaim} return the roleClaims
   */
  public List<RoleClaim> getRoleClaims() {
    return roleClaims;
  }

}
