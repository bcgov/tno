using System;

namespace TNO.Services.AutoClipper.Azure;

public record TimestampedTranscript(TimeSpan Start, TimeSpan End, string Text);
