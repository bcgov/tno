package ca.bc.gov.tno.dal.db.entities;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.JoinColumn;
import javax.persistence.ManyToOne;
import javax.persistence.Table;

import ca.bc.gov.tno.dal.db.SyndicationTypes;

@Entity
@Table(name = "\"SyndicationSources\"")
public class SyndicationSource extends Audit {
  @Id
  @GeneratedValue(strategy = GenerationType.AUTO)
  @Column(name = "\"id\"", nullable = false)
  private int id;

  @Column(name = "\"name\"", nullable = false)
  private String name;

  @Column(name = "\"abbr\"", nullable = false)
  private String abbr;

  @Column(name = "\"description\"")
  private String description;

  @Column(name = "\"isEnabled\"", nullable = false)
  private boolean isEnabled;

  @Column(name = "\"url\"", nullable = false)
  private String url;

  @Column(name = "\"typeId\"", nullable = false)
  private SyndicationTypes typeId;

  @ManyToOne()
  @JoinColumn(name = "\"dataSourceId\"")
  private DataSource dataSource;

  public SyndicationSource() {

  }

  public SyndicationSource(int id, String name, SyndicationTypes type, String url, DataSource source) {
    this.id = id;
    this.name = name;
    this.typeId = type;
    this.url = url;
    this.dataSource = source;
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
   * @return String return the abbr
   */
  public String getAbbr() {
    return abbr;
  }

  /**
   * @param abbr the abbr to set
   */
  public void setAbbr(String abbr) {
    this.abbr = abbr;
  }

  /**
   * @return String return the url
   */
  public String getUrl() {
    return url;
  }

  /**
   * @param url the url to set
   */
  public void setUrl(String url) {
    this.url = url;
  }

  /**
   * @return SyndicationTypes return the typeId
   */
  public SyndicationTypes getTypeId() {
    return typeId;
  }

  /**
   * @param typeId the typeId to set
   */
  public void setTypeId(SyndicationTypes typeId) {
    this.typeId = typeId;
  }

  /**
   * @return DataSource return the dataSource
   */
  public DataSource getDataSource() {
    return dataSource;
  }

  /**
   * @param dataSource the dataSource to set
   */
  public void setDataSource(DataSource dataSource) {
    this.dataSource = dataSource;
  }

}
