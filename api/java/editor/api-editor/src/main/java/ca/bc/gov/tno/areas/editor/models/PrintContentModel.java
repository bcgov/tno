package ca.bc.gov.tno.areas.editor.models;

import ca.bc.gov.tno.dal.db.entities.PrintContent;

/**
 * PrintContentModel class, provides a serializable model.
 */
public class PrintContentModel extends ContentModel {
  private int contentId;
  private String edition;
  private String section;
  private String storyType;
  private String byline;

  /**
   * Creates a new instance of a PrintContentModel object.
   */
  public PrintContentModel() {
  }

  /**
   * Creates a new instance of a PrintContentModel object, initializes with
   * specified
   * parameter.
   *
   * @param entity
   */
  public PrintContentModel(PrintContent entity) {
    super(entity.getContent());

    if (entity != null) {
      this.contentId = entity.getContentId();
      this.edition = entity.getEdition();
      this.section = entity.getSection();
      this.storyType = entity.getStoryType();
      this.byline = entity.getByline();
    }
  }

  /**
   * Cast model to entity.
   *
   * @return A new instance of a Content object.
   */
  public PrintContent ToPrintContent() {
    var content = super.ToContent();
    var print = new PrintContent(content, this.edition, this.section, this.storyType, this.byline);

    print.setContentId(this.getId());

    return print;
  }

  /**
   * @return int return the contentId
   */
  public int getContentId() {
    return contentId;
  }

  /**
   * @param contentId the contentId to set
   */
  public void setContentId(int contentId) {
    this.contentId = contentId;
  }

  /**
   * @return String return the edition
   */
  public String getEdition() {
    return edition;
  }

  /**
   * @param edition the edition to set
   */
  public void setEdition(String edition) {
    this.edition = edition;
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
   * @return String return the storyType
   */
  public String getStoryType() {
    return storyType;
  }

  /**
   * @param storyType the storyType to set
   */
  public void setStoryType(String storyType) {
    this.storyType = storyType;
  }

  /**
   * @return String return the byline
   */
  public String getByline() {
    return byline;
  }

  /**
   * @param byline the byline to set
   */
  public void setByline(String byline) {
    this.byline = byline;
  }

}
