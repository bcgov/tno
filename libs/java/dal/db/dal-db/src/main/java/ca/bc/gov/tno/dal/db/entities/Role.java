package ca.bc.gov.tno.dal.db.entities;

import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.OneToMany;
import javax.persistence.Table;

import com.fasterxml.jackson.annotation.JsonBackReference;

/**
 * Role class, provides a way to manage roles.
 */
@Entity
@Table(name = "\"Role\"")
public class Role extends AuditColumns {
  /**
   * Primary key to identify the role.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.AUTO)
  @Column(name = "\"id\"", nullable = false)
  private int id;

  /**
   * A unique name to identify the role.
   */
  @Column(name = "\"name\"", nullable = false)
  private String name;

  /**
   * A unique key to identify the role.
   */
  @Column(name = "\"key\"", nullable = false)
  private UUID key;

  /**
   * A description of the role.
   */
  @Column(name = "\"description\"")
  private String description;

  /**
   * Whether this record is enabled or disabled.
   */
  @Column(name = "\"isEnabled\"", nullable = false)
  private boolean isEnabled;

  /**
   * A collection of user roles that belong to this role.
   */
  @JsonBackReference
  @OneToMany(mappedBy = "role", fetch = FetchType.LAZY)
  private List<UserRole> userRoles = new ArrayList<>();

  /**
   * A collection of role claims that belong to this role.
   */
  @OneToMany(mappedBy = "role", fetch = FetchType.LAZY)
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
   * @param id   Primary key
   * @param name Unique name
   */
  public Role(int id, String name) {
    this.id = id;
    this.name = name;
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
   * @return boolean return the isEnabled
   */
  public boolean isIsEnabled() {
    return isEnabled;
  }

  /**
   * @param isEnabled the isEnabled to set
   */
  public void setIsEnabled(boolean isEnabled) {
    this.isEnabled = isEnabled;
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
