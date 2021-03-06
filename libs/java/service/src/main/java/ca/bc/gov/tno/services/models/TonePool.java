package ca.bc.gov.tno.services.models;

import java.util.ArrayList;
import java.util.List;

/**
 * TonePool class, provides a way to manage tone pools.
 */
public class TonePool extends AuditColumns {
  /**
   * Primary key to identify the tone pool.
   */
  private int id;

  /**
   * A unique name to identify the tone pool.
   */
  private String name;

  /**
   * A description of the tone pool.
   */
  private String description = "";

  /**
   * Foreign key to User who owns this tone pool.
   */
  private int ownerId;

  /**
   * The owner reference.
   */
  private User owner;

  /**
   * The order to display.
   */
  private int sortOrder;

  /**
   * Whether this record is visible to other users.
   */
  private boolean shared = true;

  /**
   * Whether this record is enabled or disabled.
   */
  private boolean isEnabled = true;

  /**
   * A collection of role tone pools that belong to this tone pool.
   */
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
   * @param name   Unique name
   * @param owner  User object who owns this tone pool
   * @param shared Whether this tone pool is shared with everyone
   */
  public TonePool(String name, User owner, boolean shared) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");
    if (owner == null)
      throw new NullPointerException("Parameter 'owner' cannot be null.");

    this.name = name;
    this.owner = owner;
    this.ownerId = owner.getId();
    this.shared = shared;
  }

  /**
   * Creates a new instance of a TonePool object, initializes with specified
   * parameters.
   *
   * @param id     Primary key
   * @param name   Unique name
   * @param owner  User object who owns this tone pool
   * @param shared Whether this tone pool is shared with everyone
   */
  public TonePool(int id, String name, User owner, boolean shared) {
    this(name, owner, shared);
    this.id = id;
  }

  /**
   * Creates a new instance of a TonePool object, initializes with specified
   * parameters.
   *
   * @param id      Primary key
   * @param name    Unique name
   * @param owner   User object who owns this tone pool
   * @param shared  Whether this tone pool is shared with everyone
   * @param version Row version value
   */
  public TonePool(int id, String name, User owner, boolean shared, long version) {
    this(id, name, owner, shared);
    this.setVersion(version);
  }

  /**
   * Creates a new instance of a TonePool object, initializes with specified
   * parameters.
   *
   * @param name    Unique name
   * @param ownerId Foreign key to user who owns the tone pool
   * @param shared  Whether this tone pool is shared with everyone
   */
  public TonePool(String name, int ownerId, boolean shared) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");

    this.name = name;
    this.ownerId = ownerId;
    this.shared = shared;
  }

  /**
   * Creates a new instance of a TonePool object, initializes with specified
   * parameters.
   *
   * @param id      Primary key
   * @param name    Unique name
   * @param ownerId Foreign key to user who owns the tone pool
   * @param shared  Whether this tone pool is shared with everyone
   */
  public TonePool(int id, String name, int ownerId, boolean shared) {
    this(name, ownerId, shared);
    this.id = id;
  }

  /**
   * Creates a new instance of a TonePool object, initializes with specified
   * parameters.
   *
   * @param id      Primary key
   * @param name    Unique name
   * @param ownerId Foreign key to user who owns the tone pool
   * @param shared  Whether this tone pool is shared with everyone
   * @param version Row version value
   */
  public TonePool(int id, String name, int ownerId, boolean shared, long version) {
    this(id, name, ownerId, shared);
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
