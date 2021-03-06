package ca.bc.gov.tno.areas.editor.models;

import java.time.ZonedDateTime;
import java.util.ArrayList;
import java.util.List;

import javax.persistence.Persistence;

import ca.bc.gov.tno.dal.db.ContentStatus;
import ca.bc.gov.tno.dal.db.entities.Content;
import ca.bc.gov.tno.dal.db.entities.ContentAction;
import ca.bc.gov.tno.dal.db.entities.ContentCategory;
import ca.bc.gov.tno.dal.db.entities.ContentTag;
import ca.bc.gov.tno.dal.db.entities.ContentTone;
import ca.bc.gov.tno.dal.db.entities.FileReference;
import ca.bc.gov.tno.dal.db.entities.TimeTracking;
import ca.bc.gov.tno.dal.db.WorkflowStatus;
import ca.bc.gov.tno.models.AuditColumnModel;

/**
 * ContentModel class, provides a serializable model.
 */
public class ContentModel extends AuditColumnModel {
  private int id;
  private ContentStatus status;
  private WorkflowStatus workflowStatus;
  private int contentTypeId;
  private ContentTypeModel contentType;
  private int mediaTypeId;
  private MediaTypeModel mediaType;
  private int licenseId;
  private LicenseModel license;
  private Integer seriesId;
  private SeriesModel series;
  private int ownerId;
  private UserModel owner;
  private Integer dataSourceId;
  private DataSourceModel dataSource;
  private String source;
  private String headline;
  private String uid = "";
  private String page = "";
  private String summary = "";
  private String transcription = "";
  private ZonedDateTime publishedOn;
  private String sourceURL = "";
  private List<FileReferenceModel> fileReferences = new ArrayList<FileReferenceModel>();
  private List<CategoryModel> categories = new ArrayList<CategoryModel>();
  private List<TagModel> tags = new ArrayList<TagModel>();
  private List<TonePoolModel> tonePools = new ArrayList<TonePoolModel>();
  private List<ActionModel> actions = new ArrayList<ActionModel>();
  private List<TimeTrackingModel> timeTrackings = new ArrayList<TimeTrackingModel>();

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
      var putil = Persistence.getPersistenceUtil();

      this.id = entity.getId();
      this.status = entity.getStatus();
      this.workflowStatus = entity.getWorkflowStatus();
      this.contentTypeId = entity.getContentTypeId();
      if (putil.isLoaded(entity, "contentType"))
        this.contentType = new ContentTypeModel(entity.getContentType());
      this.mediaTypeId = entity.getMediaTypeId();
      if (putil.isLoaded(entity, "mediaType"))
        this.mediaType = new MediaTypeModel(entity.getMediaType());
      this.licenseId = entity.getLicenseId();
      if (putil.isLoaded(entity, "license"))
        this.license = new LicenseModel(entity.getLicense());
      if (entity.getSeriesId() != null) {
        this.seriesId = entity.getSeriesId();
        if (putil.isLoaded(entity, "series"))
          this.series = new SeriesModel(entity.getSeries());
      }
      this.ownerId = entity.getOwnerId();
      if (putil.isLoaded(entity, "owner"))
        this.owner = new UserModel(entity.getOwner());
      this.dataSourceId = entity.getDataSourceId();
      if (putil.isLoaded(entity, "dataSource"))
        this.dataSource = entity.getDataSource() != null ? new DataSourceModel(entity.getDataSource()) : null;
      this.source = entity.getSource();
      this.headline = entity.getHeadline();
      this.uid = entity.getUid();
      this.page = entity.getPage();
      this.summary = entity.getSummary();
      this.transcription = entity.getTranscription();
      this.publishedOn = entity.getPublishedOn();
      this.sourceURL = entity.getSourceURL();

      if (putil.isLoaded(entity, "fileReferences")) {
        this.fileReferences = entity.getFileReferences().stream()
            .map((file) -> new FileReferenceModel(file))
            .toList();
      }
      if (putil.isLoaded(entity, "contentCategories")) {
        this.categories = entity.getContentCategories().stream()
            .map((category) -> new CategoryModel(category.getCategory(), category.getScore(), category.getVersion()))
            .toList();
      }
      if (putil.isLoaded(entity, "contentTags")) {
        this.tags = entity.getContentTags().stream()
            .map((tag) -> new TagModel(tag.getTag(), tag.getVersion()))
            .toList();
      }
      if (putil.isLoaded(entity, "contentTonePools")) {
        this.tonePools = entity.getContentTonePools().stream()
            .map((tone) -> new TonePoolModel(tone.getTonePool(), tone.getValue(), tone.getVersion()))
            .toList();
      }
      if (putil.isLoaded(entity, "contentActions")) {
        this.actions = entity.getContentActions().stream()
            .map((action) -> new ActionModel(action.getAction(), action.getValue(), action.getVersion()))
            .toList();
      }
      if (putil.isLoaded(entity, "timeTrackings")) {
        this.timeTrackings = entity.getTimeTrackings().stream()
            .map((time) -> new TimeTrackingModel(time))
            .toList();
      }
    }
  }

  /**
   * Cast model to entity.
   *
   * @return A new instance of a Content object.
   */
  public Content ToContent() {
    var content = new Content(
        this.getId(),
        this.contentTypeId,
        this.mediaTypeId,
        this.licenseId,
        this.seriesId,
        this.source,
        this.ownerId,
        this.status,
        this.headline,
        this.getVersion());

    content.setDataSourceId(this.dataSourceId);
    content.setUid(this.uid);
    content.setPage(this.page);
    content.setSummary(this.summary);
    content.setTranscription(this.transcription);
    content.setPublishedOn(this.publishedOn);
    content.setSourceURL(this.sourceURL);
    content.getFileReferences()
        .addAll(this.fileReferences.stream()
            .map((file) -> new FileReference(file.getId(), content, file.getPath(), file.getMimeType(), file.getSize(),
                file.getRunningTime(), file.getVersion()))
            .toList());
    content.getContentActions()
        .addAll(this.actions.stream()
            .map((action) -> new ContentAction(content, action.getId(), action.getValue(), action.getVersion()))
            .toList());
    content.getContentTags()
        .addAll(this.tags.stream()
            .map((tag) -> new ContentTag(content, tag.getId(), tag.getVersion()))
            .toList());
    content.getContentCategories()
        .addAll(this.categories.stream()
            .map((category) -> new ContentCategory(content, category.getId(), category.getScore(),
                category.getVersion()))
            .toList());
    content.getContentTonePools()
        .addAll(this.tonePools.stream()
            .map((tone) -> new ContentTone(content, tone.getId(), tone.getValue(), tone.getVersion()))
            .toList());
    content.getTimeTrackings()
        .addAll(this.timeTrackings.stream()
            .map((time) -> new TimeTracking(content, time.getUserId(), time.getEffort(), time.getActivity(),
                time.getVersion()))
            .toList());

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
   * @return WorkflowStatus return the workflowStatus
   */
  public WorkflowStatus getWorkflowStatus() {
    return workflowStatus;
  }

  /**
   * @param workflowStatus the workflowStatus to set
   */
  public void setWorkflowStatus(WorkflowStatus workflowStatus) {
    this.workflowStatus = workflowStatus;
  }

  /**
   * @return Integer return the seriesId
   */
  public Integer getSeriesId() {
    return seriesId;
  }

  /**
   * @param seriesId the seriesId to set
   */
  public void setSeriesId(Integer seriesId) {
    this.seriesId = seriesId;
  }

  /**
   * @return SeriesModel return the series
   */
  public SeriesModel getSeries() {
    return series;
  }

  /**
   * @param series the series to set
   */
  public void setSeries(SeriesModel series) {
    this.series = series;
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
   * @return ZonedDateTime return the publishedOn
   */
  public ZonedDateTime getPublishedOn() {
    return publishedOn;
  }

  /**
   * @param publishedOn the publishedOn to set
   */
  public void setPublishedOn(ZonedDateTime publishedOn) {
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

  /**
   * @return List{FileReferenceModel} return the fileReferences
   */
  public List<FileReferenceModel> getFileReferences() {
    return fileReferences;
  }

  /**
   * @param fileReferences the fileReferences to set
   */
  public void setFileReferences(List<FileReferenceModel> fileReferences) {
    this.fileReferences = fileReferences;
  }

  /**
   * @return List{TagModel} return the tags
   */
  public List<TagModel> getTags() {
    return tags;
  }

  /**
   * @param tags the tags to set
   */
  public void setTags(List<TagModel> tags) {
    this.tags = tags;
  }

  /**
   * @return List<CategoryModel> return the categories
   */
  public List<CategoryModel> getCategories() {
    return categories;
  }

  /**
   * @param categories the categories to set
   */
  public void setCategories(List<CategoryModel> categories) {
    this.categories = categories;
  }

  /**
   * @return List<TonePoolModel> return the tonePools
   */
  public List<TonePoolModel> getTonePools() {
    return tonePools;
  }

  /**
   * @param tonePools the tonePools to set
   */
  public void setTonePools(List<TonePoolModel> tonePools) {
    this.tonePools = tonePools;
  }

  /**
   * @return List<ActionModel> return the actions
   */
  public List<ActionModel> getActions() {
    return actions;
  }

  /**
   * @param actions the actions to set
   */
  public void setActions(List<ActionModel> actions) {
    this.actions = actions;
  }

  /**
   * @return List<TimeTrackingModel> return the timeTrackings
   */
  public List<TimeTrackingModel> getTimeTrackings() {
    return timeTrackings;
  }

  /**
   * @param timeTrackings the timeTrackings to set
   */
  public void setTimeTrackings(List<TimeTrackingModel> timeTrackings) {
    this.timeTrackings = timeTrackings;
  }

}
