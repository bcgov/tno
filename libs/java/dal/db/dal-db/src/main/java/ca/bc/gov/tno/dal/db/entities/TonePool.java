package ca.bc.gov.tno.dal.db.entities;

import java.util.ArrayList;
import java.util.List;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.JoinColumn;
import javax.persistence.ManyToOne;
import javax.persistence.OneToMany;
import javax.persistence.SequenceGenerator;
import javax.persistence.Table;

import com.fasterxml.jackson.annotation.JsonBackReference;

import ca.bc.gov.tno.dal.db.AuditColumns;

/**
 * TonePool class, provides a way to manage tone pools.
 */
@Entity
@Table(name = "\"TonePool\"")
public class TonePool extends AuditColumns {
  /**
   * Primary key to identify the tone pool.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_TonePool")
  @SequenceGenerator(name = "seq_TonePool", allocationSize = 1)
  @Column(name = "\"id\"", nullable = false)
  private int id;

  /**
   * A unique name to identify the tone pool.
   */
  @Column(name = "\"name\"", nullable = false)
  private String name;

  /**
   * A description of the tone pool.
   */
  @Column(name = "\"description\"")
  private String description = "";

  /**
   * Foreign key to User who owns this tone pool.
   */
  @Column(name = "\"ownerId\"", nullable = false)
  private int ownerId;

  /**
   * The owner reference.
   */
  @JsonBackReference
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "\"ownerId\"", insertable = false, updatable = false)
  private User owner;

  /**
   * The order to display.
   */
  @Column(name = "\"sortOrder\"", nullable = false)
  private int sortOrder;

  /**
   * Whether this record is visible to other users.
   */
  @Column(name = "\"isPublic\"", nullable = false)
  private boolean shared = true;

  /**
   * Whether this record is enabled or disabled.
   */
  @Column(name = "\"isEnabled\"", nullable = false)
  private boolean enabled = true;

  /**
   * A collection of role tone pools that belong to this tone pool.
   */
  @JsonBackReference
  @OneToMany(mappedBy = "tonePool", fetch = FetchType.LAZY)
  private List<ContentTone> contentTones = new ArrayList<>();

  /**
   * Creates a new instance of a TonePool object.
   */
  public TonePool() {

  }

  /**
   * Creates a new instance of a TonePool object, initializes with specified
   * parameters.
   * 
   * @param id     Primary key
   * @param name   Unique name
   * @param owner  User object
   * @param shared User object
   */
  public TonePool(int id, String name, User owner, boolean shared) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");
    if (owner == null)
      throw new NullPointerException("Parameter 'owner' cannot be null.");

    this.id = id;
    this.name = name;
    this.owner = owner;
    this.ownerId = owner.getId();
    this.shared = shared;
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
   * @return int return the ownerId
   */
  public int getOwnerId() {
    return ownerId;
  }

  /**
   * @param ownerId the ownerId to set
   */
  public void setOwnerId(int ownerId) {
    this.ownerId = ownerId;
  }

  /**
   * @return User return the owner
   */
  public User getOwner() {
    return owner;
  }

  /**
   * @param owner the owner to set
   */
  public void setOwner(User owner) {
    this.owner = owner;
  }

  /**
   * @return int return the sortOrder
   */
  public int getSortOrder() {
    return sortOrder;
  }

  /**
   * @param sortOrder the sortOrder to set
   */
  public void setSortOrder(int sortOrder) {
    this.sortOrder = sortOrder;
  }

  /**
   * @return boolean return the shared
   */
  public boolean isShared() {
    return shared;
  }

  /**
   * @param shared the shared to set
   */
  public void setShared(boolean shared) {
    this.shared = shared;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setEnabled(boolean enabled) {
    this.enabled = enabled;
  }

  /**
   * @return List{ContentTone} return the contentTones
   */
  public List<ContentTone> getContentTones() {
    return contentTones;
  }

  /**
   * @param contentTones the contentTones to set
   */
  public void setContentTones(List<ContentTone> contentTones) {
    this.contentTones = contentTones;
  }

}
