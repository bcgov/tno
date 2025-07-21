namespace TNO.Tools.ElasticIndexer;

class Program
{
    static Task<int> Main(string[] args)
    {
        var program = new IndexerService(args);
        return program.RunAsync();
    }
}
