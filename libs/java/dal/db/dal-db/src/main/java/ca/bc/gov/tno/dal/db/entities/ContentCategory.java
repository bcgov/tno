package ca.bc.gov.tno.dal.db.entities;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.Id;
import javax.persistence.IdClass;
import javax.persistence.JoinColumn;
import javax.persistence.ManyToOne;
import javax.persistence.Table;

import ca.bc.gov.tno.dal.db.AuditColumns;

/**
 * ContentCategory class, provides a way to manage content categories.
 */
@Entity
@IdClass(ContentCategoryPK.class)
@Table(name = "content_category", schema = "public")
public class ContentCategory extends AuditColumns {
  /**
   * Primary key to identify the content category.
   * Foreign key to content.
   */
  @Id
  @Column(name = "content_id", nullable = false)
  private int contentId;

  /**
   * The content reference.
   */
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "content_id", insertable = false, updatable = false)
  private Content content;

  /**
   * Primary key to identify the content category.
   * Foreign key to category .
   */
  @Id
  @Column(name = "category_id", nullable = false)
  private int categoryId;

  /**
   * The category reference.
   */
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "category_id", insertable = false, updatable = false)
  private Category category;

  /**
   * Score of category.
   */
  @Column(name = "score", nullable = false)
  private int score;

  /**
   * Creates a new instance of a ContentCategory object.
   */
  public ContentCategory() {

  }

  /**
   * Creates a new instance of a ContentCategory object, initializes with
   * specified
   * parameters.
   * 
   * @param content  Content object
   * @param category Category object
   * @param score    Category score
   */
  public ContentCategory(Content content, Category category, int score) {
    if (content == null)
      throw new NullPointerException("Parameter 'content' cannot be null.");
    if (category == null)
      throw new NullPointerException("Parameter 'category' cannot be null.");

    this.content = content;
    this.contentId = content.getId();
    this.category = category;
    this.categoryId = category.getId();
    this.score = score;
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
   * @return Content return the content
   */
  public Content getContent() {
    return content;
  }

  /**
   * @param content the content to set
   */
  public void setContent(Content content) {
    this.content = content;
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

  /**
   * @return Category return the category
   */
  public Category getCategory() {
    return category;
  }

  /**
   * @param category the category to set
   */
  public void setCategory(Category category) {
    this.category = category;
  }

  /**
   * @return int return the score
   */
  public int getScore() {
    return score;
  }

  /**
   * @param score the score to set
   */
  public void setScore(int score) {
    this.score = score;
  }

}
