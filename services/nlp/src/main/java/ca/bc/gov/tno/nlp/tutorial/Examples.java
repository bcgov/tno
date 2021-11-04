package ca.bc.gov.tno.nlp.tutorial;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.util.Arrays;
import java.util.List;

import opennlp.tools.chunker.ChunkerME;
import opennlp.tools.chunker.ChunkerModel;
import opennlp.tools.langdetect.Language;
import opennlp.tools.langdetect.LanguageDetector;
import opennlp.tools.langdetect.LanguageDetectorFactory;
import opennlp.tools.langdetect.LanguageDetectorME;
import opennlp.tools.langdetect.LanguageDetectorModel;
import opennlp.tools.langdetect.LanguageDetectorSampleStream;
import opennlp.tools.lemmatizer.DictionaryLemmatizer;
import opennlp.tools.namefind.NameFinderME;
import opennlp.tools.namefind.TokenNameFinderModel;
import opennlp.tools.postag.POSModel;
import opennlp.tools.postag.POSTaggerME;
import opennlp.tools.sentdetect.SentenceDetectorME;
import opennlp.tools.sentdetect.SentenceModel;
import opennlp.tools.tokenize.SimpleTokenizer;
import opennlp.tools.tokenize.TokenizerME;
import opennlp.tools.tokenize.TokenizerModel;
import opennlp.tools.tokenize.WhitespaceTokenizer;
import opennlp.tools.util.InputStreamFactory;
import opennlp.tools.util.PlainTextByLineStream;
import opennlp.tools.util.Span;
import opennlp.tools.util.TrainingParameters;
import opennlp.tools.util.MarkableFileInputStreamFactory;

public class Examples {
  /**
   * Sentence detection is about identifying the start and the end of a sentence,
   * which usually depends on the language at hand. This is also called “Sentence
   * Boundary Disambiguation” (SBD).
   * 
   * @throws Exception
   */
  public void getSentences() throws Exception {
    String paragraph = "This is a statement. This is another statement." + "Now is an abstract word for time, "
        + "that is always flying. And my email address is google@gmail.com.";

    InputStream is = getClass().getResourceAsStream("/models/en-sent.bin");
    var model = new SentenceModel(is);

    var sdetector = new SentenceDetectorME(model);

    String sentences[] = sdetector.sentDetect(paragraph);

    for (var i = 0; i < sentences.length; i++) {
      System.out.println(sentences[i]);
    }

    // assertThat(sentences).contains(
    // "This is a statement.",
    // "This is another statement.",
    // "Now is an abstract word for time, that is always flying.",
    // "And my email address is google@gmail.com.");
  }

  /**
   * The goal of tokenization is to divide a sentence into smaller parts called
   * tokens. Usually, these tokens are words, numbers or punctuation marks.
   * 
   * @throws Exception
   */
  public void getTokens() throws Exception {
    InputStream inputStream = getClass().getResourceAsStream("/models/en-token.bin");
    var model = new TokenizerModel(inputStream);
    var tokenizer = new TokenizerME(model);
    String[] tokens = tokenizer.tokenize("Baeldung is a Spring Resource.");

    for (var i = 0; i < tokens.length; i++) {
      System.out.println(tokens[i]);
    }

    // assertThat(tokens).contains(
    // "Baeldung", "is", "a", "Spring", "Resource", ".");
  }

  /**
   * As the name suggests, this tokenizer simply splits the sentence into tokens
   * using whitespace characters as delimiters:
   * 
   * @throws Exception
   */
  public void getTokensFromWhitespace() throws Exception {
    var tokenizer = WhitespaceTokenizer.INSTANCE;
    String[] tokens = tokenizer.tokenize("Baeldung is a Spring Resource.");

    for (var i = 0; i < tokens.length; i++) {
      System.out.println(tokens[i]);
    }

    // assertThat(tokens)
    // .contains("Baeldung", "is", "a", "Spring", "Resource.");
  }

  /**
   * This tokenizer is a little more sophisticated than WhitespaceTokenizer and
   * splits the sentence into words, numbers, and punctuation marks. It's the
   * default behavior and doesn't require any model:
   * 
   * @throws Exception
   */
  public void getTokensFromSimple() throws Exception {
    var tokenizer = SimpleTokenizer.INSTANCE;
    String[] tokens = tokenizer.tokenize("Baeldung is a Spring Resource.");

    for (var i = 0; i < tokens.length; i++) {
      System.out.println(tokens[i]);
    }

    // assertThat(tokens)
    // .contains("Baeldung", "is", "a", "Spring", "Resource", ".");
  }

  /**
   * The goal of NER is to find named entities like people, locations,
   * organizations and other named things in a given text.
   * 
   * @throws Exception
   */
  public void getNER() throws Exception {
    SimpleTokenizer tokenizer = SimpleTokenizer.INSTANCE;
    String[] tokens = tokenizer
        .tokenize("John is 26 years old. His best friend's " + "name is Leonard. He has a sister named Penny.");

    InputStream inputStreamNameFinder = getClass().getResourceAsStream("/models/en-ner-person.bin");
    var model = new TokenNameFinderModel(inputStreamNameFinder);
    var nameFinderME = new NameFinderME(model);
    List<Span> spans = Arrays.asList(nameFinderME.find(tokens));

    System.out.println(spans.toString());

    String a[] = Span.spansToStrings(spans.toArray(new Span[spans.size()]), tokens);
    var sb = new StringBuilder();
    var l = a.length;

    for (var j = 0; j < l; j++) {
      sb = sb.append((a[j] + "\n"));
    }
    System.out.println(sb);

    // assertThat(spans.toString())
    // .isEqualTo("[[0..1) person, [13..14) person, [20..21) person]");
  }

  /**
   * A part-of-speech (POS) identifies the type of a word. OpenNLP uses the
   * following tags for the different parts-of-speech:
   * 
   * NN – noun, singular or mass
   * 
   * DT – determiner
   * 
   * VB – verb, base form
   * 
   * VBD – verb, past tense
   * 
   * VBZ – verb, third person singular present
   * 
   * IN – preposition or subordinating conjunction
   * 
   * NNP – proper noun, singular
   * 
   * TO – the word “to”
   * 
   * JJ – adjective
   * 
   * These are same tags as defined in the Penn Tree Bank. For a complete list
   * please refer to this list.
   * 
   * @throws Exception
   */
  public void getPOS() throws Exception {
    SimpleTokenizer tokenizer = SimpleTokenizer.INSTANCE;
    String[] tokens = tokenizer.tokenize("John has a sister named Penny.");

    InputStream inputStreamPOSTagger = getClass().getResourceAsStream("/models/en-pos-maxent.bin");
    var posModel = new POSModel(inputStreamPOSTagger);
    var posTagger = new POSTaggerME(posModel);
    String tags[] = posTagger.tag(tokens);

    for (var i = 0; i < tags.length; i++) {
      System.out.println(tags[i]);
    }

    // assertThat(tags).contains("NNP", "VBZ", "DT", "NN", "VBN", "NNP", ".");
  }

  /**
   * Lemmatization is the process of mapping a word form that can have a tense,
   * gender, mood or other information to the base form of the word – also called
   * its “lemma”.
   * 
   * A lemmatizer takes a token and its part-of-speech tag as input and returns
   * the word's lemma. Hence, before Lemmatization, the sentence should be passed
   * through a tokenizer and POS tagger.
   * 
   * Apache OpenNLP provides two types of lemmatization:
   * 
   * Statistical – needs a lemmatizer model built using training data for finding
   * the lemma of a given word
   * 
   * Dictionary-based – requires a dictionary which contains all valid
   * combinations of a word, POS tags, and the corresponding lemma
   * 
   * @throws Exception
   */
  public void getLemmatization() throws Exception {
    SimpleTokenizer tokenizer = SimpleTokenizer.INSTANCE;
    String[] tokens = tokenizer.tokenize("John has a sister named Penny.");

    InputStream inputStreamPOSTagger = getClass().getResourceAsStream("/models/en-pos-maxent.bin");
    var posModel = new POSModel(inputStreamPOSTagger);
    var posTagger = new POSTaggerME(posModel);
    String tags[] = posTagger.tag(tokens);
    InputStream dictLemmatizer = getClass().getResourceAsStream("/models/en-lemmatizer.dict");
    var lemmatizer = new DictionaryLemmatizer(dictLemmatizer);
    String[] lemmas = lemmatizer.lemmatize(tokens, tags);

    for (var i = 0; i < lemmas.length; i++) {
      System.out.println(lemmas[i]);
    }

    // assertThat(lemmas)
    // .contains("O", "have", "a", "sister", "name", "O", "O");
  }

  /**
   * Part-of-speech information is also essential in chunking – dividing sentences
   * into grammatically meaningful word groups like noun groups or verb groups.
   * 
   * @throws Exception
   */
  public void getChucks() throws Exception {
    SimpleTokenizer tokenizer = SimpleTokenizer.INSTANCE;
    String[] tokens = tokenizer.tokenize("He reckons the current account deficit will narrow to only 8 billion.");

    InputStream inputStreamPOSTagger = getClass().getResourceAsStream("/models/en-pos-maxent.bin");
    POSModel posModel = new POSModel(inputStreamPOSTagger);
    POSTaggerME posTagger = new POSTaggerME(posModel);
    String tags[] = posTagger.tag(tokens);

    InputStream inputStreamChunker = getClass().getResourceAsStream("/models/en-chunker.bin");
    var chunkerModel = new ChunkerModel(inputStreamChunker);
    var chunker = new ChunkerME(chunkerModel);
    String[] chunks = chunker.chunk(tokens, tags);

    for (var i = 0; i < chunks.length; i++) {
      System.out.println(chunks[i]);
    }

    // assertThat(chunks).contains(
    // "B-NP", "B-VP", "B-NP", "I-NP",
    // "I-NP", "I-NP", "B-VP", "I-VP",
    // "B-PP", "B-NP", "I-NP", "I-NP", "O");
  }

  /**
   * Additionally to the use cases already discussed, OpenNLP also provides a
   * language detection API that allows to identify the language of a certain
   * text.
   * 
   * For language detection, we need a training data file. Such a file contains
   * lines with sentences in a certain language. Each line is tagged with the
   * correct language to provide input to the machine learning algorithms.
   * 
   * @throws FileNotFoundException
   * @throws IOException
   */
  public void getLanguage() throws FileNotFoundException, IOException {

    InputStreamFactory dataIn = new MarkableFileInputStreamFactory(
        new File("src/main/resources/models/DoccatSample.txt"));
    var lineStream = new PlainTextByLineStream(dataIn, "UTF-8");
    LanguageDetectorSampleStream sampleStream = new LanguageDetectorSampleStream(lineStream);
    TrainingParameters params = new TrainingParameters();
    params.put(TrainingParameters.ITERATIONS_PARAM, 100);
    params.put(TrainingParameters.CUTOFF_PARAM, 5);
    params.put("DataIndexer", "TwoPass");
    params.put(TrainingParameters.ALGORITHM_PARAM, "NAIVEBAYES");

    LanguageDetectorModel model = LanguageDetectorME.train(sampleStream, params, new LanguageDetectorFactory());

    LanguageDetector ld = new LanguageDetectorME(model);
    Language[] languages = ld.predictLanguages("estava em uma marcenaria na Rua Bruno");

    for (var i = 0; i < languages.length; i++) {
      System.out.println("lang: " + languages[i].getLang() + " confidence: " + languages[i].getConfidence());
    }

    // assertThat(Arrays.asList(languages))
    // .extracting("lang", "confidence")
    // .contains(
    // tuple("pob", 0.9999999950605625),
    // tuple("ita", 4.939427661577956E-9),
    // tuple("spa", 9.665954064665144E-15),
    // tuple("fra", 8.250349924885834E-25)));
  }
}
