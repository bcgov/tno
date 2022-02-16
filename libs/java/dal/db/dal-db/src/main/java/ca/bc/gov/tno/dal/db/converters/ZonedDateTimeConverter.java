package ca.bc.gov.tno.dal.db.converters;

import java.sql.Timestamp;
import java.time.ZoneId;
import java.time.ZonedDateTime;

import javax.persistence.AttributeConverter;

public class ZonedDateTimeConverter implements AttributeConverter<ZonedDateTime, java.sql.Timestamp> {
  @Override
  public java.sql.Timestamp convertToDatabaseColumn(ZonedDateTime entityValue) {
    return entityValue == null ? null : Timestamp.valueOf(entityValue.toLocalDateTime());
  }

  @Override
  public ZonedDateTime convertToEntityAttribute(java.sql.Timestamp dbValue) {
    var local = dbValue.toLocalDateTime();
    return dbValue == null ? null : local.atZone(ZoneId.of("+00"));
  }
}
