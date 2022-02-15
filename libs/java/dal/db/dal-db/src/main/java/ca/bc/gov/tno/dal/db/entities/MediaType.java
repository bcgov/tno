package ca.bc.gov.tno.dal.db.entities;

import java.util.ArrayList;
import java.util.List;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.OneToMany;
import javax.persistence.SequenceGenerator;
import javax.persistence.Table;

import com.fasterxml.jackson.annotation.JsonIgnore;

import ca.bc.gov.tno.dal.db.AuditColumns;

/**
 * MediaType class, provides a way to identify the different media
 * types.
 */
@Entity
@Table(name = "media_type", schema = "public")
public class MediaType extends AuditColumns {
  /**
   * Primary key to identify the media type.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_media_type")
  @SequenceGenerator(name = "seq_media_type", allocationSize = 1)
  @Column(name = "id", nullable = false)
  private int id;

  /**
   * A unique name to identify the media type.
   */
  @Column(name = "name", nullable = false)
  private String name;

  /**
   * A description of the media type.
   */
  @Column(name = "description")
  private String description = "";

  /**
   * Whether this record is enabled or disabled.
   */
  @Column(name = "is_enabled", nullable = false)
  private boolean enabled = true;

  /**
   * A collection of data sources of this type.
   */
  @JsonIgnore
  @OneToMany(mappedBy = "mediaType", fetch = FetchType.LAZY)
  private List<DataSource> dataSources = new ArrayList<>();

  /**
   * Creates a new instance of a MediaType object.
   */
  public MediaType() {

  }

  /**
   * Creates a new instance of a MediaType object, initializes with specified
   * parameters.
   * 
   * @param id   Primary key
   * @param name Unique name
   */
  public MediaType(int id, String name) {
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
   * @param id the id to set
   */
  public void setId(int id) {
    this.id = id;
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
   * @return List{DataSource} return the data_sources
   */
  public List<DataSource> getDataSources() {
    return dataSources;
  }

  /**
   * @param dataSources the dataSources to set
   */
  public void setDataSources(List<DataSource> dataSources) {
    this.dataSources = dataSources;
  }

}
