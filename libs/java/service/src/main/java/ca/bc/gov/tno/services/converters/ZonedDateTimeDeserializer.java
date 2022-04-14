package ca.bc.gov.tno.services.converters;

import java.io.IOException;
import java.time.Instant;
import java.time.ZoneOffset;
import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;

import com.fasterxml.jackson.core.JsonParser;
import com.fasterxml.jackson.databind.DeserializationContext;
import com.fasterxml.jackson.databind.JsonDeserializer;

/**
 * Custom {@link ZonedDateTime} deserializer.
 */
public class ZonedDateTimeDeserializer extends JsonDeserializer<ZonedDateTime> {

  /**
   * @param jsonParser for extracting the date in {@link String}
   *                   format.
   * @param context    for the process of deserialization a single
   *                   root-level value.
   * @return {@link ZonedDateTime} object of the date.
   * @throws IOException throws I/O exceptions.
   */
  @Override
  public ZonedDateTime deserialize(JsonParser jsonParser, DeserializationContext context)
      throws IOException {

    var value = jsonParser.getText();

    if (value.equals("0001-01-01T00:00:00")) {
      var minInstant = Instant.ofEpochMilli(Long.MIN_VALUE);
      return minInstant.atZone(ZoneOffset.UTC);
    }

    // Try multiple format parsers.
    for (var format : Settings.dateTimeFormats) {
      try {
        return ZonedDateTime.parse(jsonParser.getText(), DateTimeFormatter.ofPattern(format));
      } catch (Exception ex) {
        // Ignore
      }
    }

    // Retry default format so we get an error.
    return ZonedDateTime.parse(jsonParser.getText(), DateTimeFormatter.ofPattern(Settings.dateTimeFormat));
  }
}
