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
import javax.persistence.SequenceGenerator;
import javax.persistence.Table;

import com.fasterxml.jackson.annotation.JsonManagedReference;

import ca.bc.gov.tno.dal.db.AuditColumns;

/**
 * Claim class, provides a way to manage claims.
 */
@Entity
@Table(name = "claim", schema = "public")
public class Claim extends AuditColumns {
  /**
   * Primary key to identify the claim.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_claim")
  @SequenceGenerator(name = "seq_claim", allocationSize = 1)
  @Column(name = "id", nullable = false)
  private int id;

  /**
   * A unique name to identify the claim.
   */
  @Column(name = "name", nullable = false)
  private String name;

  /**
   * A unique key to identify the claim.
   */
  @Column(name = "key", nullable = false)
  private UUID key;

  /**
   * A description of the claim.
   */
  @Column(name = "description")
  private String description = "";

  /**
   * Whether this record is enabled or disabled.
   */
  @Column(name = "is_enabled", nullable = false)
  private boolean enabled = true;

  /**
   * A collection of role claims that belong to this claim.
   */
  @JsonManagedReference("claim")
  @OneToMany(mappedBy = "claim", fetch = FetchType.LAZY)
  private List<RoleClaim> roleClaims = new ArrayList<>();

  /**
   * Creates a new instance of a Claim object.
   */
  public Claim() {

  }

  /**
   * Creates a new instance of a Claim object, initializes with specified
   * parameters.
   * 
   * @param id   Primary key
   * @param name Unique name
   */
  public Claim(int id, String name) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");

    this.id = id;
    this.name = name;
  }

  /**
   * Creates a new instance of a Claim object, initializes with specified
   * parameters.
   * 
   * @param id      Primary key
   * @param name    Unique name
   * @param version Row version value
   */
  public Claim(int id, String name, int version) {
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
   * @param roleClaims the roleClaims to set
   */
  public void setRoleClaims(List<RoleClaim> roleClaims) {
    this.roleClaims = roleClaims;
  }

}
