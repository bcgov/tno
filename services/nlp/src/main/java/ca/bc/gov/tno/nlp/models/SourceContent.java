package ca.bc.gov.tno.nlp.models;

/**
 * Content class, provides a model for capturing all the relevant data that will
 * make content searchable.
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
   * @param source
   * @param type
   * @param uid
   * @param title
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
