package ca.bc.gov.tno.services.converters;

/**
 * Settings static class, provides configuration settings.
 */
public class Settings {
  /**
   * Default DateTime format. 2022-04-11T18:56:55.253908Z
   */
  public final static String dateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.SSSSSSX";

  /**
   * Handle multiple format types.
   * This is ridiculous, but java is horrible.
   */
  public final static String[] dateTimeFormats = new String[] {
      dateTimeFormat,
      "yyyy-MM-dd'T'HH:mm:ss.SSSSSX",
      "yyyy-MM-dd'T'HH:mm:ss.SSSSX",
      "yyyy-MM-dd'T'HH:mm:ss.SSSX",
      "yyyy-MM-dd'T'HH:mm:ss.SSX",
      "yyyy-MM-dd'T'HH:mm:ssX" };
}
