using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TNO.Entities.Validation;

/// <summary>
/// KafkaTopicAttribute class, provides validation for Kafka topics.
/// </summary>
public class KafkaTopicAttribute : ValidationAttribute
{
    #region Methods
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is Ingest ingest)
        {
            if ((ingest.Schedules.Any() || ingest.SchedulesManyToMany.Any()) && String.IsNullOrWhiteSpace(ingest.Topic))
                return new ValidationResult("Topic is required if a schedule has been provided.");

            if (!String.IsNullOrWhiteSpace(ingest.Topic))
            {
                if (ingest.Topic.Length > 255) return new ValidationResult("Topic can only be a maximum of 255 characters");

                var match = Regex.Match(ingest.Topic, @"^[a-zA-Z0-9._-]+$");
                if (!match.Success) return new ValidationResult("Topic can only include the following characters: a-z, A-Z, 0-9, . (dot), _ (underscore), - (dash)");
            }

        }

        return ValidationResult.Success;
    }
    #endregion
}
