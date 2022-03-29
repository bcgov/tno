package ca.bc.gov.tno.areas.admin.models;

import javax.persistence.Persistence;

import ca.bc.gov.tno.dal.db.entities.DataSourceAction;

public class DataSourceActionModel extends SourceActionModel {

  /**
   * The value given to the action.
   */
  private boolean value;

  public DataSourceActionModel() {
  }

  public DataSourceActionModel(DataSourceAction entity) {
    if (entity != null) {
      var putil = Persistence.getPersistenceUtil();

      if (putil.isLoaded(entity, "sourceAction")) {
        var action = entity.getSourceAction();
        this.setId(action.getId());
        this.setName(action.getName());
        this.setDescription(action.getDescription());
        this.setEnabled(action.isEnabled());
        this.setSortOrder(action.getSortOrder());
      }
      this.value = entity.getValue();
      this.setVersion(entity.getVersion());
    }
  }

  /**
   * @return boolean the value
   */
  public boolean getValue() {
    return value;
  }

  /**
   * @param sortOrder the value to set
   */
  public void setValue(boolean value) {
    this.value = value;
  }

}
