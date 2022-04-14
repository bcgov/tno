package ca.bc.gov.tno.services.data;

/**
 * ApiException class, provides a custom exception for API requests.
 */
public final class ApiException extends Exception {

  /**
   * Creates a new instance of an ApiException object, initializes with
   * parameters.
   * 
   * @param errorMessage The error message.
   */
  public ApiException(String errorMessage) {
    super(errorMessage);
  }
}
