package ca.bc.gov.tno;

/**
 * IEnumValue interface, provides a way to ensure enum objects can extract data
 * for each value.
 */
public interface IEnumValue<T extends java.io.Serializable> {
  /**
   * Get the value stored in the enum key.
   * 
   * @return The enum value.
   */
  public T getValue();
}
