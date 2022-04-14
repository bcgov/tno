package ca.bc.gov.tno.services.converters;

import java.time.ZoneId;
import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;
import java.util.Date;

/**
 * Helper methods to parse ZonedDateTime values.
 */
public class ParseZonedDateTime {
  /**
   * Attempts to parse the string 'value' into a ZonedDateTime.
   * 
   * @param value The date value to parse.
   * @return A new instance of a ZonedDateTime, or an error.
   */
  public static ZonedDateTime parse(String value) {
    for (var format : Settings.dateTimeFormats) {
      try {
        return ZonedDateTime.parse(value, DateTimeFormatter.ofPattern(format));
      } catch (Exception ex) {
        // Ignore...
      }
    }

    // Use the default to throw a relevant error.
    return ZonedDateTime.parse(value, DateTimeFormatter.ofPattern(Settings.dateTimeFormat));
  }

  /**
   * Convert ZonedDateTime to String with default format.
   * 
   * @param value The ZonedDateTime value.
   * @return A formatted date string.
   */
  public static String format(ZonedDateTime value) {
    return value.format(DateTimeFormatter.ofPattern(Settings.dateTimeFormat));
  }

  /**
   * Convert a Date 'value' to a ZonedDateTime and then a string with the default
   * format.
   * 
   * @param value The Date value.
   * @return A formatted date string.
   */
  public static String format(Date value) {
    return toZone(value).format(DateTimeFormatter.ofPattern(Settings.dateTimeFormat));
  }

  /**
   * Convert a Date 'value' to a ZonedDateTime with the default system zone.
   * 
   * @param value The Date value.
   * @return A ZonedDateTime value.
   */
  public static ZonedDateTime toZone(Date value) {
    return value.toInstant().atZone(ZoneId.systemDefault());
  }
}
