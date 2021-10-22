package ca.bc.gov.tno.dal.db.converters;

import java.util.EnumSet;
import java.util.stream.IntStream;

import javax.persistence.AttributeConverter;

import ca.bc.gov.tno.dal.db.entities.IEnumValue;

/**
 * Convert an EnumSet to and from an Integer. This supports Flags, which enables
 * bitwise operator comparisons.
 */
public class EnumSetAttributeConverter<T extends Enum<T>> implements AttributeConverter<EnumSet<T>, Integer> {
  private Class<T> typeParameterClass;

  @Override
  public Integer convertToDatabaseColumn(EnumSet<T> attribute) {
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
  public EnumSet<T> convertToEntityAttribute(Integer dbData) {
    if (dbData == null)
      return null;

    var all = getValues(typeParameterClass);
    var values = EnumSet.noneOf(typeParameterClass);
    for (var a : all) {
      var value = ((IEnumValue) a).getValue();
      if (value == (dbData & value)) {
        values.add(a);
      }
    }

    return values;
  }

  /**
   * Get all the enum values from the specified type.
   * 
   * @param <T>      Type of enum
   * @param enumType The enum
   * @return An array of the enum values
   */
  public static <T extends Enum<T>> T[] getValues(Class<T> enumType) {
    return (T[]) enumType.getEnumConstants();
  }

}
