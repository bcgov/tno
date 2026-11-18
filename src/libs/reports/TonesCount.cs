namespace TNO.Reports;

class TonesCount
{
    public TonesCount()
    {
        this.Neutral = 0;
        this.Positive = 0;
        this.Negative = 0;
    }
    public int Neutral { get; set; }
    public int Positive { get; set; }
    public int Negative { get; set; }
}
