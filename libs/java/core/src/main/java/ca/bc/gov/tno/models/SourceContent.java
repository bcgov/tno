package ca.bc.gov.tno.models;

import ca.bc.gov.tno.ContentType;

/**
 * SourceContent class, provides a model to capture source content in a uniform
 * way. This will enable consumers to be written in a standard format.
 */
public class SourceContent extends ContentBase {

  /**
   * The content body;
   */
  private String body;

  /**
   * Creates a new instance of a Content object.
   */
  public SourceContent() {
    super();
  }

  /**
   * Creates a new instance of a SourceContent object, initializes with specified
   * parameters.
   * 
   * @param source The unique identifier of the source (i.e. code).
   * @param type   The type of content [text|audio|video].
   * @param uid    A unique identifier for the content.
   * @param title  The content title.
   */
  public SourceContent(String source, ContentType type, String uid, String title) {
    super(source, type, uid, title);
  }

  /**
   * @return String return the body
   */
  public String getBody() {
    return body;
  }

  /**
   * @param body the body to set
   */
  public void setBody(String body) {
    this.body = body;
  }

}
