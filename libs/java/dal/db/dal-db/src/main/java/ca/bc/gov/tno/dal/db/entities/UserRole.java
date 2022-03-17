package ca.bc.gov.tno.dal.db.entities;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.Id;
import javax.persistence.JoinColumn;
import javax.persistence.ManyToOne;
import javax.persistence.Table;

import com.fasterxml.jackson.annotation.JsonBackReference;
import com.fasterxml.jackson.annotation.JsonIgnore;

import ca.bc.gov.tno.dal.db.AuditColumns;

/**
 * UserRole class, provides a way to manage user roles.
 */
@Entity
@Table(name = "user_role", schema = "public")
public class UserRole extends AuditColumns {
  /**
   * Primary key and foreign key to the user.
   */
  @Id
  @Column(name = "user_id", nullable = false)
  private int userId;

  /**
   * The user reference.
   */
  @JsonIgnore
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "user_id", insertable = false, updatable = false)
  private User user;

  /**
   * Primary key and foreign key to the role.
   */
  @Id
  @Column(name = "role_id", nullable = false)
  private int roleId;

  /**
   * The role reference.
   */
  @JsonBackReference("userRole")
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "role_id", insertable = false, updatable = false)
  private Role role;

  /**
   * Creates a new instance of a UserRole object.
   */
  public UserRole() {

  }

  /**
   * Creates a new instance of a UserRole object, initializes with specified
   * parameters.
   * 
   * @param user The user.
   * @param role The role.
   */
  public UserRole(User user, Role role) {
    if (user == null)
      throw new IllegalArgumentException("Parameter 'user' is required.");
    if (role == null)
      throw new IllegalArgumentException("Parameter 'role' is required.");

    this.user = user;
    this.userId = user.getId();
    this.role = role;
    this.roleId = role.getId();
  }

  /**
   * Creates a new instance of a UserRole object, initializes with specified
   * parameters.
   * 
   * @param user    The user.
   * @param role    The role.
   * @param version Row version value
   */
  public UserRole(User user, Role role, long version) {
    this(user, role);
    this.setVersion(version);
  }

  /**
   * Creates a new instance of a UserRole object, initializes with specified
   * parameters.
   * 
   * @param userId Foreign key to The user.
   * @param roleId Foreign key to The role.
   */
  public UserRole(int userId, int roleId) {
    this.userId = userId;
    this.roleId = roleId;
  }

  /**
   * Creates a new instance of a UserRole object, initializes with specified
   * parameters.
   * 
   * @param userId  Foreign key to The user.
   * @param roleId  Foreign key to The role.
   * @param version Row version value
   */
  public UserRole(int userId, int roleId, long version) {
    this(userId, roleId);
    this.setVersion(version);
  }

  /**
   * @return int return the userId
   */
  public int getUserId() {
    return userId;
  }

  /**
   * @param userId the userId to set
   */
  public void setUserId(int userId) {
    this.userId = userId;
  }

  /**
   * @return User return the user
   */
  public User getUser() {
    return user;
  }

  /**
   * @param user the user to set
   */
  public void setUser(User user) {
    this.user = user;
  }

  /**
   * @return int return the roleId
   */
  public int getRoleId() {
    return roleId;
  }

  /**
   * @param roleId the roleId to set
   */
  public void setRoleId(int roleId) {
    this.roleId = roleId;
  }

  /**
   * @return Role return the role
   */
  public Role getRole() {
    return role;
  }

  /**
   * @param role the role to set
   */
  public void setRole(Role role) {
    this.role = role;
  }

}
