using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TNO.Elastic.Models.Converters;

public class AggregationsModelConverter : JsonConverter<AggregationsModel>
{
    const string PropNameDocCount =  "doc_count";
    const string PropNameDocCountErrorUpperBound =  "doc_count_error_upper_bound";
    const string PropNameSumOtherDocCount =  "sum_other_doc_count";
    const string PropNameBuckets =  "buckets";

    public override AggregationsModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var dto = new AggregationsModel();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dto;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                // kgm: is it worth return the aggregrate name from the search?
                // string propName = (reader.GetString() ?? "").ToLower();
                reader.Read();

                ParseAggregationDetail(ref reader, dto);
            }
        }

        throw new JsonException();
    }
    private static void ParseAggregationDetail(ref Utf8JsonReader reader, AggregationsModel dto)
    {
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
                string propName = (reader.GetString() ?? "").ToLower();
                reader.Read();

                switch (propName)
                {
                    case var _ when propName.Equals(PropNameDocCount.ToLower()):
                        dto.DocCount = reader.GetInt64();
                        break;
                    case var _ when propName.Equals(PropNameSumOtherDocCount.ToLower()):
                        dto.OtherDocCount = reader.GetInt64();
                        break;
                    case var _ when propName.Equals(PropNameBuckets.ToLower()):
                        using (var jsonDoc = JsonDocument.ParseValue(ref reader))
                        {
                            var buckets = jsonDoc.RootElement.GetRawText();
                            dto.Buckets = System.Text.Json.JsonSerializer.Deserialize<List<AggregationBucketModel>>(buckets);
                        }
                        break;
                    case var _ when propName.Equals(PropNameDocCountErrorUpperBound.ToLower()):
                        break;
                    default:
                        ParseAggregationDetail(ref reader, dto);
                        break;
                }

        }
    }

    public override void Write(Utf8JsonWriter writer, AggregationsModel value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.DocCount.HasValue)
            writer.WriteNumber(PropNameDocCount, value.DocCount.Value);

        if (value.OtherDocCount.HasValue)
            writer.WriteNumber(PropNameSumOtherDocCount, value.OtherDocCount.Value);

        writer.WriteStartArray(PropNameBuckets);
        if (value.Buckets.Any()) {
            foreach(AggregationBucketModel bucket in value!.Buckets) {
                JsonSerializer.Serialize(writer, bucket, options);
            }
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}
