package ca.bc.gov.tno.services.nlp.parsers;

import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashSet;
import java.util.List;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.CompletionException;
import java.util.concurrent.ExecutionException;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;
import org.springframework.context.annotation.Scope;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.models.SourceContent;
import ca.bc.gov.tno.models.Tag;
import ca.bc.gov.tno.services.events.ErrorEvent;
import ca.bc.gov.tno.services.nlp.events.ContentParsedEvent;
import ca.bc.gov.tno.services.nlp.events.ContentReadyEvent;
import ca.bc.gov.tno.TagKey;
import opennlp.tools.namefind.NameFinderME;
import opennlp.tools.namefind.TokenNameFinderModel;
import opennlp.tools.tokenize.SimpleTokenizer;
import opennlp.tools.util.Span;

/**
 * NlpParser class, provides a way to perform Natural Language Processing on
 * content.
 */
@Async
@Component
@Scope("prototype")
public class NlpParser implements ApplicationListener<ContentParsedEvent> {
  private static final Logger logger = LogManager.getLogger(ContentParser.class);

  private final ApplicationEventPublisher eventPublisher;

  /**
   * Creates a new instance of a NlpParser object, initializes with specified
   * parameters.
   * 
   * @param eventPublisher Application event publisher.
   */
  @Autowired
  public NlpParser(final ApplicationEventPublisher eventPublisher) {
    this.eventPublisher = eventPublisher;
  }

  /**
   * Parse content and extract keywords, Name Entity Recognition (NER).
   * 
   * @param event The source event.
   */
  @Override
  public void onApplicationEvent(ContentParsedEvent event) {
    try {
      var content = event.getContent();
      var result = event.getResult();

      logger.debug(String.format("NLP processing '%s'", content.getUid()));
      process(content, result.getTags());
      logger.info(String.format("NLP processed '%s'", content.getUid()));

      var readyEvent = new ContentReadyEvent(this, result);
      eventPublisher.publishEvent(readyEvent);
    } catch (IOException | InterruptedException | ExecutionException ex) {
      // TODO: Failed content needs to identified so that it can be rerun. Or it needs
      // to be pushed as it is into the next queue.
      var errorEvent = new ErrorEvent(this, ex);
      eventPublisher.publishEvent(errorEvent);
    }
  }

  /**
   * Perform each NLP action and update the content.
   * 
   * @param content
   * @param tags
   * @throws IOException
   * @throws ExecutionException
   * @throws InterruptedException
   */
  private void process(SourceContent content, List<Tag> tags)
      throws IOException, InterruptedException, ExecutionException {
    SimpleTokenizer tokenizer = SimpleTokenizer.INSTANCE;
    var text = content.getBody() != null && !content.getBody().trim().isEmpty() ? content.getBody()
        : content.getSummary();
    String[] tokens = tokenizer.tokenize(text);

    var language = content.getLanguage() == null || content.getLanguage().trim().isEmpty() ? "en"
        : content.getLanguage().trim().toLowerCase();
    content.setLanguage(language);

    var persons = CompletableFuture
        .supplyAsync(() -> extractNERAsync(tokens, String.format("/models/%s-ner-person.bin", language)));
    var organizations = CompletableFuture
        .supplyAsync(() -> extractNERAsync(tokens, String.format("/models/%s-ner-organization.bin", language)));
    var locations = CompletableFuture
        .supplyAsync(() -> extractNERAsync(tokens, String.format("/models/%s-ner-location.bin", language)));
    var dates = CompletableFuture
        .supplyAsync(() -> extractNERAsync(tokens, String.format("/models/%s-ner-date.bin", language)));

    // TODO: extract keywords and phrases.

    var combinedFutures = CompletableFuture.allOf(persons, organizations, locations, dates);
    combinedFutures.get();

    addTags(tags, TagKey.person, persons.get().get());
    addTags(tags, TagKey.organization, organizations.get().get());
    addTags(tags, TagKey.location, locations.get().get());
    addTags(tags, TagKey.date, dates.get().get());
  }

  /**
   * Add tags to the content for the specified 'key'.
   * 
   * @param content
   * @param key
   * @param values
   */
  private void addTags(List<Tag> tags, TagKey key, List<String> values) {
    var distinct = new ArrayList<>(new HashSet<>(values));
    distinct.forEach((value) -> {
      var tag = new Tag(key, value);
      tags.add(tag);
    });
  }

  /**
   * Extract Named Entity Recognition objects from the specified 'tokens', with
   * the resource model at the specified 'pathToResource'.
   * 
   * @param tokens
   * @param pathToResource
   * @return
   * @throws CompletionException
   */
  private CompletableFuture<List<String>> extractNERAsync(String[] tokens, String pathToResource)
      throws CompletionException {
    try {
      return CompletableFuture.completedFuture(extractNER(tokens, pathToResource));
    } catch (IOException ex) {
      throw new CompletionException(ex);
    }
  }

  /**
   * Extract Named Entity Recognition objects from the specified 'tokens', with
   * the resource model at the specified 'pathToResource'.
   * 
   * @param tokens
   * @param pathToResource
   * @return
   * @throws IOException
   */
  private List<String> extractNER(String[] tokens, String pathToResource) throws IOException {
    InputStream inputStreamNameFinder = getClass().getResourceAsStream(pathToResource);
    var model = new TokenNameFinderModel(inputStreamNameFinder);
    var nameFinderME = new NameFinderME(model);
    List<Span> spans = Arrays.asList(nameFinderME.find(tokens));

    return Arrays.asList(Span.spansToStrings(spans.toArray(new Span[spans.size()]), tokens));
  }
}
