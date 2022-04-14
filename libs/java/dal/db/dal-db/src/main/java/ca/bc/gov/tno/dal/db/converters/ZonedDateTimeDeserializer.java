package ca.bc.gov.tno.dal.db.converters;

import java.io.IOException;
import java.time.Instant;
import java.time.ZoneOffset;
import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;

import com.fasterxml.jackson.core.JsonParser;
import com.fasterxml.jackson.databind.DeserializationContext;
import com.fasterxml.jackson.databind.JsonDeserializer;

import ca.bc.gov.tno.dal.db.services.Settings;

/**
 * Custom {@link ZonedDateTime} deserializer.
 *
 * @param jsonParser for extracting the date in {@link String}
 *                   format.
 * @param context    for the process of deserialization a single
 *                   root-level value.
 * @return {@link ZonedDateTime} object of the date.
 * @throws IOException throws I/O exceptions.
 */
public class ZonedDateTimeDeserializer extends JsonDeserializer<ZonedDateTime> {
  @Override
  public ZonedDateTime deserialize(JsonParser jsonParser, DeserializationContext context)
      throws IOException {

    var value = jsonParser.getText();

    if (value.equals("0001-01-01T00:00:00")) {
      var minInstant = Instant.ofEpochMilli(Long.MIN_VALUE);
      return minInstant.atZone(ZoneOffset.UTC);
    }

    return ZonedDateTime.parse(jsonParser.getText(), DateTimeFormatter.ofPattern(Settings.dateTimeFormat));
  }
}
