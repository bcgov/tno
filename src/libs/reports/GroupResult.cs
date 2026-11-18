namespace TNO.Reports;

class GroupResult
{
    public string Name { get; set; }
    public string Date { get; set; }
    public int Count { get; set; }
    public double Tone { get; set; }

    public GroupResult(string name, string date, int count, double tone)
    {
        this.Name = name;
        this.Date = date;
        this.Count = count;
        this.Tone = tone;
    }
}
