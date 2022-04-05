package ca.bc.gov.tno.areas.editor.models;

import ca.bc.gov.tno.dal.db.entities.FileReference;
import ca.bc.gov.tno.models.AuditColumnModel;

public class FileReferenceModel extends AuditColumnModel {
  /**
   * Primary key to identify the file reference.
   */
  private int id;

  /**
   * The file mime-type.
   */
  private String mimeType;

  /**
   * The path to the file.
   */
  private String path;

  /**
   * The size of the file in bytes.
   */
  private int size;

  /**
   * The length of the video/audio in milliseconds.
   */
  private int runningTime;

  public FileReferenceModel() {
  }

  public FileReferenceModel(FileReference entity) {
    super(entity);

    if (entity != null) {
      this.id = entity.getId();
      this.mimeType = entity.getMimeType();
      this.path = entity.getPath();
      this.size = entity.getSize();
      this.runningTime = entity.getRunningTime();
    }
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
   * @return String return the mimeType
   */
  public String getMimeType() {
    return mimeType;
  }

  /**
   * @param mimeType the mimeType to set
   */
  public void setMimeType(String mimeType) {
    this.mimeType = mimeType;
  }

  /**
   * @return String return the path
   */
  public String getPath() {
    return path;
  }

  /**
   * @param path the path to set
   */
  public void setPath(String path) {
    this.path = path;
  }

  /**
   * @return int return the size
   */
  public int getSize() {
    return size;
  }

  /**
   * @param size the size to set
   */
  public void setSize(int size) {
    this.size = size;
  }

  /**
   * @return int return the runningTime
   */
  public int getRunningTime() {
    return runningTime;
  }

  /**
   * @param runningTime the runningTime to set
   */
  public void setRunningTime(int runningTime) {
    this.runningTime = runningTime;
  }

}
