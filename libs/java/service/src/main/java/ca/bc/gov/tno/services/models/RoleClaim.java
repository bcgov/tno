package ca.bc.gov.tno.services.models;

/**
 * RoleClaim class, provides a way to manage claim roles.
 */
public class RoleClaim extends AuditColumns {
  /**
   * Primary key and foreign key to the role.
   */
  private int roleId;

  /**
   * The role reference.
   */
  private Role role;

  /**
   * Primary key and foreign key to the claim.
   */
  private int claimId;

  /**
   * The claim reference.
   */
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
  public RoleClaim(Role role, Claim claim, long version) {
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
  public RoleClaim(int roleId, int claimId, long version) {
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
