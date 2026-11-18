namespace TNO.Reports;

class HeadlineResult
{
    public double? Tone { get; set; }
    public string Date { get; set; }
    public string Headline { get; set; }
    public string Type { get; set; }
    public string? Source { get; set; }
    public string? Series { get; set; }

    public HeadlineResult(double? tone, string date, string headline, string type, string? source, string? series)
    {
        this.Tone = tone;
        this.Date = date;
        this.Headline = headline;
        this.Type = type;
        this.Source = source;
        this.Series = series;
    }
}
