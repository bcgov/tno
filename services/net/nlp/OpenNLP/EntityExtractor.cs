namespace TNO.Services.NLP.OpenNLP;

/// <summary>
/// Extractor for the entity types available in openNLP.
/// </summary>
public class EntityExtractor
{
    #region Variables
    private readonly string sentenceModelPath = "./models/en-sent.bin";   //path to the model for sentence detection
    private readonly string tokenModelPath = "./models/en-token.bin";     //model path for English tokens
    private string? nameFinderModelPath;                              //NameFinder model path for English names
    #endregion

    #region Methods
    /// <summary>
    /// Extract the specified entity type from the specified 'text'.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public string[] ExtractEntities(string text, EntityType entity)
    {
        /*required steps to detect names are:
         * downloaded sentence, token, and name models from http://opennlp.sourceforge.net/models-1.5/
         * 1. Parse the input into sentences
         * 2. Parse the sentences into tokens
         * 3. Find the entity in the tokens
        */

        // TODO: Configurable mapping to models.
        nameFinderModelPath = entity switch
        {
            EntityType.Date => "./models/en-ner-date.bin",
            EntityType.Location => "./models/en-ner-location.bin",
            EntityType.Money => "./models/en-ner-money.bin",
            EntityType.Organization => "./models/en-ner-organization.bin",
            EntityType.Person => "./models/en-ner-person.bin",
            EntityType.Time => "./models/en-ner-time.bin",
            _ => throw new InvalidOperationException("Parameter 'targetType' does not have a configured model."),
        };

        // Load modules.
        var sentenceParser = PrepareSentenceDetector();
        var nameFinder = PrepareNameFinder();
        var tokenizer = PrepareTokenizer();

        // Extract sentences.
        var sentences = sentenceParser.sentDetect(text);
        var results = new List<string>();

        foreach (string sentence in sentences)
        {
            var tokens = tokenizer.tokenize(sentence);
            var foundNames = nameFinder.find(tokens);

            // important:  clear adaptive data in the feature generators or the detection rate will decrease over time.
            nameFinder.clearAdaptiveData();

            results.AddRange(opennlp.tools.util.Span.spansToStrings(foundNames, tokens).AsEnumerable().Select(v => CleanText(v)));
        }

        return results.ToArray();
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Clean up the text from known issues.
    /// Spans often contain closing periods.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static string CleanText(string text)
    {
        if (text.EndsWith(".")) return text.Remove(text.Length - 1).Trim();
        return text;
    }

    private opennlp.tools.tokenize.TokenizerME PrepareTokenizer()
    {
        var tokenInputStream = new java.io.FileInputStream(tokenModelPath);
        var tokenModel = new opennlp.tools.tokenize.TokenizerModel(tokenInputStream);
        return new opennlp.tools.tokenize.TokenizerME(tokenModel);
    }

    private opennlp.tools.sentdetect.SentenceDetectorME PrepareSentenceDetector()
    {
        var sentModelStream = new java.io.FileInputStream(sentenceModelPath);
        var sentModel = new opennlp.tools.sentdetect.SentenceModel(sentModelStream);
        return new opennlp.tools.sentdetect.SentenceDetectorME(sentModel);
    }

    private opennlp.tools.namefind.NameFinderME PrepareNameFinder()
    {
        var modelInputStream = new java.io.FileInputStream(nameFinderModelPath);
        var model = new opennlp.tools.namefind.TokenNameFinderModel(modelInputStream);
        return new opennlp.tools.namefind.NameFinderME(model);
    }
    #endregion
}
