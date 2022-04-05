package ca.bc.gov.tno.areas.admin.models;

import javax.persistence.Persistence;

import ca.bc.gov.tno.dal.db.entities.TonePool;
import ca.bc.gov.tno.models.AuditColumnModel;

public class TonePoolModel extends AuditColumnModel {
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
  private UserModel owner;

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
  private boolean enabled = true;

  /**
   * The value given to the tone.
   */
  private int value;

  /**
   * A collection of role tone pools that belong to this tone pool.
   */
  // private List<ContentTone> contentTones = new ArrayList<>();

  public TonePoolModel() {
  }

  public TonePoolModel(TonePool entity) {
    this(entity, 0);
  }

  public TonePoolModel(TonePool entity, int value) {
    super(entity);

    if (entity != null) {
      var putil = Persistence.getPersistenceUtil();

      this.id = entity.getId();
      this.name = entity.getName();
      this.description = entity.getDescription();
      this.ownerId = entity.getOwnerId();
      if (putil.isLoaded(entity, "owner"))
        this.owner = new UserModel(entity.getOwner());
      this.shared = entity.isShared();
      this.enabled = entity.getIsEnabled();
      this.sortOrder = entity.getSortOrder();
    }
    this.value = value;
  }

  /**
   * @return int the id
   */
  public int getId() {
    return id;
  }

  /**
   * @param id the id to set
   */
  public void setId(int id) {
    this.id = id;
  }

  /**
   * @return String the name
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
   * @return String the description
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
   * @return boolean the enabled
   */
  public boolean getIsEnabled() {
    return enabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setIsEnabled(boolean enabled) {
    this.enabled = enabled;
  }

  /**
   * @return int the sortOrder
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
   * @return int the ownerId
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
   * @return UserModel the owner
   */
  public UserModel getOwner() {
    return owner;
  }

  /**
   * @param owner the owner to set
   */
  public void setOwner(UserModel owner) {
    this.owner = owner;
  }

  /**
   * @return boolean the shared
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
   * @return int return the value
   */
  public int getValue() {
    return value;
  }

  /**
   * @param value the value to set
   */
  public void setValue(int value) {
    this.value = value;
  }

}
