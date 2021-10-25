package ca.bc.gov.tno.dal.db.converters;

import java.util.EnumSet;
import java.util.stream.IntStream;

import javax.persistence.AttributeConverter;

import ca.bc.gov.tno.dal.db.Months;
import ca.bc.gov.tno.IEnumValue;

/**
 * Convert an EnumSet to and from an Integer. This supports Flags, which enables
 * bitwise operator comparisons.
 */
public class MonthsAttributeConverter implements AttributeConverter<EnumSet<Months>, Integer> {

  @Override
  public Integer convertToDatabaseColumn(EnumSet<Months> attribute) {
    if (attribute == null)
      return null;

    var values = new int[attribute.size()];
    var index = 0;
    for (var a : attribute) {
      values[index++] = ((IEnumValue) a).getValue();
    }
    var sum = IntStream.of(values).sum();
    return sum;
  }

  @Override
  public EnumSet<Months> convertToEntityAttribute(Integer dbData) {
    if (dbData == null)
      return null;

    var all = Months.values();
    var values = EnumSet.noneOf(Months.class);
    for (var a : all) {
      var value = ((IEnumValue) a).getValue();
      if (value == (dbData & value)) {
        values.add(a);
      }
    }

    return values;
  }
}
