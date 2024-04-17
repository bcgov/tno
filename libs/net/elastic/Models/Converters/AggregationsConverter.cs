using System.Text.Json;
using System.Text.Json.Serialization;

namespace TNO.Elastic.Models.Converters;

public class AggregationsConverter : JsonConverter<Dictionary<string, AggregationRootModel>>
{
    const string PropNameBuckets = "buckets";
    const string PropNameDocCount = "doc_count";
    const string PropNameDocCountErrorUpperBound = "doc_count_error_upper_bound";
    const string PropNameKey = "key";
    const string PropNameAggSum = "agg_sum";
    const string PropNameValue = "value";
    const string PropNameSumOtherDocCount = "sum_other_doc_count";

    public override Dictionary<string, AggregationRootModel>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var dto = new Dictionary<string, AggregationRootModel>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dto;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string aggregateName = reader.GetString() ?? "";
                reader.Read();
                dto.Add(aggregateName, ParseAggregationRootModels(ref reader, aggregateName));
            }
        }

        throw new JsonException();
    }

    private static AggregationRootModel ParseAggregationRootModels(ref Utf8JsonReader reader, string aggregateName)
    {
        var dto = new AggregationRootModel { Name = aggregateName };

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            string propName = reader.GetString() ?? "";
            reader.Read();
            switch (propName.ToLower())
            {
                case var _ when propName.Equals(PropNameDocCount.ToLower()):
                    dto.DocCount = reader.GetInt64(); // this result is not reliable
                    break;
                default:
                    dto.ChildAggregation = ParseAggregationModel(ref reader, propName);
                    break;
            }
        }
        // calculate the sum DocCount of ChildAggregation.Buckets
        if (dto.ChildAggregation != null && dto.ChildAggregation?.Buckets.Any() == true)
            dto.DocCount = dto.ChildAggregation?.Buckets.Sum(b => b.DocCount) ?? 0;
        return dto;
    }

    private static AggregationModel ParseAggregationModel(ref Utf8JsonReader reader, string aggregateName)
    {
        var dto = new AggregationModel { Name = aggregateName };

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            string propName = reader.GetString() ?? "";
            reader.Read();
            switch (propName.ToLower())
            {
                case var _ when propName.Equals(PropNameSumOtherDocCount):
                    dto.SumOtherDocCount = reader.GetInt64();
                    break;
                case var _ when propName.Equals(PropNameDocCountErrorUpperBound):
                    dto.DocCountErrorUpperBound = reader.GetInt64();
                    break;
                case var _ when propName.Equals(PropNameBuckets):
                    dto.Buckets = ParseAggregationBucketModels(ref reader);
                    break;
            }
        }
        return dto;
    }

    private static AggregationSumModel ParseAggregationSumModel(ref Utf8JsonReader reader)
    {
        var dto = new AggregationSumModel {  };

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            string propName = reader.GetString() ?? "";
            reader.Read();
            switch (propName.ToLower())
            {
                case var _ when propName.Equals(PropNameValue):
                    dto.Value = reader.GetDouble();
                    break;
            }
        }
        return dto;
    }

    private static IEnumerable<AggregationBucketModel> ParseAggregationBucketModels(ref Utf8JsonReader reader)
    {
        var buckets = new List<AggregationBucketModel>();
        AggregationBucketModel bucket = new();
        reader.Read();

        // if the array is empty?
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            return buckets;
        }

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if ((reader.TokenType != JsonTokenType.StartObject)
                && (reader.TokenType != JsonTokenType.EndObject))
            {
                string propName = reader.GetString() ?? "";
                reader.Read();
                switch (propName.ToLower())
                {
                    case var _ when propName.Equals(PropNameDocCount.ToLower()):
                        bucket.DocCount = reader.GetInt64(); // unreliable
                        break;
                    case var _ when propName.Equals(PropNameKey.ToLower()):
                        bucket.Key = reader.GetString() ?? "";
                        break;
                    case var _ when reader.TokenType == JsonTokenType.StartObject:
                        if (propName.Equals(PropNameAggSum))
                        {
                            bucket.AggregationSum = ParseAggregationSumModel(ref reader);
                        } else {
                            bucket.ChildAggregation = ParseAggregationModel(ref reader, propName);
                        }
                        break;
                }
                if (bucket.ChildAggregation != null && bucket.ChildAggregation.Buckets.Any())
                    bucket.DocCount = bucket.ChildAggregation.Buckets.Sum(b => b.DocCount);
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                bucket = new AggregationBucketModel();
            }
            else if (reader.TokenType == JsonTokenType.EndObject)
            {
                buckets.Add(bucket);
            }
        }
        return buckets;
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, AggregationRootModel> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var aggregation in value)
        {
            writer.WriteStartObject(aggregation.Key);
            WriteAggregationRootModel(writer, aggregation.Value, options);
            writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }

    public void WriteAggregationRootModel(Utf8JsonWriter writer, AggregationRootModel value, JsonSerializerOptions options)
    {
        writer.WriteNumber(PropNameDocCount, value.DocCount);

        if (value.ChildAggregation != null)
            WriteAggregationModel(writer, value.ChildAggregation, options);
    }

    public void WriteAggregationModel(Utf8JsonWriter writer, AggregationModel value, JsonSerializerOptions options)
    {
        writer.WriteStartObject(value.Name);

        if (value.DocCountErrorUpperBound != null)
            writer.WriteNumber(PropNameDocCountErrorUpperBound, value.DocCountErrorUpperBound.Value);

        if (value.SumOtherDocCount != null)
            writer.WriteNumber(PropNameSumOtherDocCount, value.SumOtherDocCount.Value);

        writer.WriteStartArray(PropNameBuckets);
        if (value.Buckets != null && value.Buckets.Any())
        {
            foreach (AggregationBucketModel bucket in value!.Buckets)
            {
                WriteAggregationBucketModel(writer, bucket, options);
            }
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
    }

    public void WriteAggregationBucketModel(Utf8JsonWriter writer, AggregationBucketModel value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(PropNameKey, value.Key);
        writer.WriteNumber(PropNameDocCount, value.DocCount);

        if (value.AggregationSum != null)
        {
            WriteAggregationSumModel(writer, value.AggregationSum, options);
        }

        if (value.ChildAggregation != null)
        {
            WriteAggregationModel(writer, value.ChildAggregation, options);
        }

        writer.WriteEndObject();
    }

    public void WriteAggregationSumModel(Utf8JsonWriter writer, AggregationSumModel value, JsonSerializerOptions options)
    {
        writer.WriteStartObject(PropNameAggSum);

        writer.WriteNumber(PropNameValue, value.Value);

        writer.WriteEndObject();
    }
}
