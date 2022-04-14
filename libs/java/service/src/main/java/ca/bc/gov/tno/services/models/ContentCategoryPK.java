package ca.bc.gov.tno.services.models;

import java.io.Serializable;
import java.util.Objects;

/**
 * ContentCategoryPK class, provides primary key for ContentCategory.
 */
public class ContentCategoryPK implements Serializable {
  /**
   * The content category abbreviation.
   * Foreign key to content.
   */
  private int contentId;

  /**
   * The content category unique key.
   * Foreign key to category.
   */
  private int categoryId;

  /**
   * Creates a new instance of a ContentCategoryPK object.
   */
  public ContentCategoryPK() {

  }

  /**
   * Creates a new instance of a ContentCategoryPK object, initializes with
   * specified parameters.
   * 
   * @param contentId  Foreign key to content.
   * @param categoryId Foreign key to category.
   */
  public ContentCategoryPK(int contentId, int categoryId) {
    this.contentId = contentId;
    this.categoryId = categoryId;
  }

  @Override
  public int hashCode() {
    int hash = 7;
    hash = 79 * hash + Objects.hashCode(this.contentId);
    hash = 79 & hash + Objects.hashCode(this.categoryId);
    return hash;
  }

  @Override
  public boolean equals(Object obj) {
    if (this == obj) {
      return true;
    }
    if (obj == null) {
      return false;
    }
    if (getClass() != obj.getClass()) {
      return false;
    }
    final ContentCategoryPK pk = (ContentCategoryPK) obj;
    if (!Objects.equals(this.contentId, pk.contentId) || !Objects.equals(this.categoryId, pk.categoryId)) {
      return false;
    }
    return Objects.equals(this.contentId, pk.contentId) && Objects.equals(this.categoryId, pk.categoryId);
  }

  @Override
  public String toString() {
    final StringBuilder sb = new StringBuilder("{");
    sb.append("contentId=").append(contentId);
    sb.append(", categoryId=").append(categoryId);
    sb.append("}");
    return sb.toString();
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
   * @return int return the categoryId
   */
  public int getCategoryId() {
    return categoryId;
  }

  /**
   * @param categoryId the categoryId to set
   */
  public void setCategoryId(int categoryId) {
    this.categoryId = categoryId;
  }

}
