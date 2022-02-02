package ca.bc.gov.tno.areas.editor.models;

import java.util.Date;

import ca.bc.gov.tno.dal.db.ContentStatus;
import ca.bc.gov.tno.dal.db.entities.Content;
import ca.bc.gov.tno.models.AuditColumnModel;

/**
 * ContentModel class, provides a serializable model.
 */
public class ContentModel extends AuditColumnModel {
  private int id;
  private ContentStatus status;
  private int contentTypeId;
  private ContentTypeModel contentType;
  private String headline;
  private Integer dataSourceId;
  private DataSourceModel dataSource;
  private String source;
  private String uid = "";
  private int licenseId;
  private LicenseModel license;
  private int mediaTypeId;
  private MediaTypeModel mediaType;
  private String page = "";
  private String section = "";
  private String summary = "";
  private String transcription = "";
  private int ownerId;
  private UserModel owner;
  private Date publishedOn;
  private String sourceURL = "";

  /**
   * Creates a new instance of a ContentModel object.
   */
  public ContentModel() {
  }

  /**
   * Creates a new instance of a ContentModel object, initializes with specified
   * parameter.
   *
   * @param entity
   */
  public ContentModel(Content entity) {
    super(entity);

    if (entity != null) {
      this.id = entity.getId();
      this.status = entity.getStatus();
      this.contentTypeId = entity.getContentTypeId();
      this.contentType = new ContentTypeModel(entity.getContentType());
      this.headline = entity.getHeadline();
      this.dataSourceId = entity.getDataSourceId();
      this.dataSource = entity.getDataSource() != null ? new DataSourceModel(entity.getDataSource()) : null;
      this.source = entity.getSource();
      this.uid = entity.getUid();
      this.licenseId = entity.getLicenseId();
      this.license = new LicenseModel(entity.getLicense());
      this.mediaTypeId = entity.getMediaTypeId();
      this.mediaType = new MediaTypeModel(entity.getMediaType());
      this.page = entity.getPage();
      this.section = entity.getSection();
      this.summary = entity.getSummary();
      this.transcription = entity.getTranscription();
      this.ownerId = entity.getOwnerId();
      this.owner = new UserModel(entity.getOwner());
      this.publishedOn = entity.getPublishedOn();
      this.sourceURL = entity.getSourceURL();
    }
  }

  /**
   * Cast model to entity.
   *
   * @return A new instance of a Content object.
   */
  public Content Convert() {
    var content = new Content(0, this.contentTypeId, this.mediaTypeId, this.licenseId, this.source, this.ownerId,
        this.status, this.headline);

    content.setDataSourceId(this.dataSourceId);
    content.setUid(this.uid);
    content.setPage(this.page);
    content.setSection(this.section);
    content.setSummary(this.summary);
    content.setTranscription(this.transcription);
    content.setPublishedOn(this.publishedOn);
    content.setSourceURL(this.sourceURL);
    content.setMediaTypeId(this.mediaTypeId);
    content.setOwnerId(this.ownerId);

    return content;
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
   * @return ContentStatus return the status
   */
  public ContentStatus getStatus() {
    return status;
  }

  /**
   * @param status the status to set
   */
  public void setStatus(ContentStatus status) {
    this.status = status;
  }

  /**
   * @return int return the contentTypeId
   */
  public int getContentTypeId() {
    return contentTypeId;
  }

  /**
   * @param contentTypeId the contentTypeId to set
   */
  public void setContentTypeId(int contentTypeId) {
    this.contentTypeId = contentTypeId;
  }

  /**
   * @return ContentTypeModel return the contentType
   */
  public ContentTypeModel getContentType() {
    return contentType;
  }

  /**
   * @param contentType the contentType to set
   */
  public void setContentType(ContentTypeModel contentType) {
    this.contentType = contentType;
  }

  /**
   * @return String return the headline
   */
  public String getHeadline() {
    return headline;
  }

  /**
   * @param headline the headline to set
   */
  public void setHeadline(String headline) {
    this.headline = headline;
  }

  /**
   * @return Integer return the dataSourceId
   */
  public Integer getDataSourceId() {
    return dataSourceId;
  }

  /**
   * @param dataSourceId the dataSourceId to set
   */
  public void setDataSourceId(Integer dataSourceId) {
    this.dataSourceId = dataSourceId;
  }

  /**
   * @return DataSourceModel return the dataSource
   */
  public DataSourceModel getDataSource() {
    return dataSource;
  }

  /**
   * @param dataSource the dataSource to set
   */
  public void setDataSource(DataSourceModel dataSource) {
    this.dataSource = dataSource;
  }

  /**
   * @return String return the source
   */
  public String getSource() {
    return source;
  }

  /**
   * @param source the source to set
   */
  public void setSource(String source) {
    this.source = source;
  }

  /**
   * @return String return the uid
   */
  public String getUid() {
    return uid;
  }

  /**
   * @param uid the uid to set
   */
  public void setUid(String uid) {
    this.uid = uid;
  }

  /**
   * @return int return the licenseId
   */
  public int getLicenseId() {
    return licenseId;
  }

  /**
   * @param licenseId the licenseId to set
   */
  public void setLicenseId(int licenseId) {
    this.licenseId = licenseId;
  }

  /**
   * @return LicenseModel return the license
   */
  public LicenseModel getLicense() {
    return license;
  }

  /**
   * @param license the license to set
   */
  public void setLicense(LicenseModel license) {
    this.license = license;
  }

  /**
   * @return int return the mediaTypeId
   */
  public int getMediaTypeId() {
    return mediaTypeId;
  }

  /**
   * @param mediaTypeId the mediaTypeId to set
   */
  public void setMediaTypeId(int mediaTypeId) {
    this.mediaTypeId = mediaTypeId;
  }

  /**
   * @return MediaTypeModel return the mediaType
   */
  public MediaTypeModel getMediaType() {
    return mediaType;
  }

  /**
   * @param mediaType the mediaType to set
   */
  public void setMediaType(MediaTypeModel mediaType) {
    this.mediaType = mediaType;
  }

  /**
   * @return String return the page
   */
  public String getPage() {
    return page;
  }

  /**
   * @param page the page to set
   */
  public void setPage(String page) {
    this.page = page;
  }

  /**
   * @return String return the section
   */
  public String getSection() {
    return section;
  }

  /**
   * @param section the section to set
   */
  public void setSection(String section) {
    this.section = section;
  }

  /**
   * @return String return the summary
   */
  public String getSummary() {
    return summary;
  }

  /**
   * @param summary the summary to set
   */
  public void setSummary(String summary) {
    this.summary = summary;
  }

  /**
   * @return String return the transcription
   */
  public String getTranscription() {
    return transcription;
  }

  /**
   * @param transcription the transcription to set
   */
  public void setTranscription(String transcription) {
    this.transcription = transcription;
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
   * @return UserModel return the owner
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
   * @return Date return the publishedOn
   */
  public Date getPublishedOn() {
    return publishedOn;
  }

  /**
   * @param publishedOn the publishedOn to set
   */
  public void setPublishedOn(Date publishedOn) {
    this.publishedOn = publishedOn;
  }

  /**
   * @return String return the sourceURL
   */
  public String getSourceURL() {
    return sourceURL;
  }

  /**
   * @param sourceURL the sourceURL to set
   */
  public void setSourceURL(String sourceURL) {
    this.sourceURL = sourceURL;
  }

}
