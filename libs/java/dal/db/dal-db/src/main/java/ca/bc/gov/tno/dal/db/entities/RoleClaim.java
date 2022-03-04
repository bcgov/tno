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
 * RoleClaim class, provides a way to manage claim roles.
 */
@Entity
@Table(name = "role_claim", schema = "public")
public class RoleClaim extends AuditColumns {
  /**
   * Primary key and foreign key to the role.
   */
  @Id
  @Column(name = "role_id", nullable = false)
  private int roleId;

  /**
   * The role reference.
   */
  @JsonIgnore
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "role_id", insertable = false, updatable = false)
  private Role role;

  /**
   * Primary key and foreign key to the claim.
   */
  @Id
  @Column(name = "claim_id", nullable = false)
  private int claimId;

  /**
   * The claim reference.
   */
  @JsonBackReference("claim")
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "claim_id", insertable = false, updatable = false)
  private Claim claim;

  /**
   * Creates a new instance of a RoleClaim object.
   */
  public RoleClaim() {

  }

  /**
   * Creates a new instance of a RoleClaim object, initializes with specified
   * parameters.
   * 
   * @param role  The role.
   * @param claim The claim.
   */
  public RoleClaim(Role role, Claim claim) {
    if (role == null)
      throw new IllegalArgumentException("Parameter 'role' is required.");
    if (claim == null)
      throw new IllegalArgumentException("Parameter 'claim' is required.");

    this.role = role;
    this.roleId = role.getId();
    this.claim = claim;
    this.claimId = claim.getId();
  }

  /**
   * Creates a new instance of a RoleClaim object, initializes with specified
   * parameters.
   * 
   * @param role    The role.
   * @param claim   The claim.
   * @param version Row version value
   */
  public RoleClaim(Role role, Claim claim, int version) {
    if (role == null)
      throw new IllegalArgumentException("Parameter 'role' is required.");
    if (claim == null)
      throw new IllegalArgumentException("Parameter 'claim' is required.");

    this.role = role;
    this.roleId = role.getId();
    this.claim = claim;
    this.claimId = claim.getId();
    this.setVersion(version);
  }

  /**
   * Creates a new instance of a RoleClaim object, initializes with specified
   * parameters.
   * 
   * @param roleId  Foreign key The role.
   * @param claimId Foreign key The claim.
   */
  public RoleClaim(int roleId, int claimId) {
    this.roleId = roleId;
    this.claimId = claimId;
  }

  /**
   * Creates a new instance of a RoleClaim object, initializes with specified
   * parameters.
   * 
   * @param roleId  Foreign key The role.
   * @param claimId Foreign key The claim.
   * @param version Row version value
   */
  public RoleClaim(int roleId, int claimId, int version) {
    this.roleId = roleId;
    this.claimId = claimId;
    this.setVersion(version);
  }

  /**
   * @return int return the claimId
   */
  public int getClaimId() {
    return claimId;
  }

  /**
   * @param claimId the claimId to set
   */
  public void setClaimId(int claimId) {
    this.claimId = claimId;
  }

  /**
   * @return Claim return the claim
   */
  public Claim getClaim() {
    return claim;
  }

  /**
   * @param claim the claim to set
   */
  public void setClaim(Claim claim) {
    this.claim = claim;
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
