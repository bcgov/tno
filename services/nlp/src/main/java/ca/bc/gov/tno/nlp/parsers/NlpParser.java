package ca.bc.gov.tno.nlp.parsers;

import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashSet;
import java.util.List;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;
import org.springframework.context.annotation.Scope;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.nlp.events.ContentParsedEvent;
import ca.bc.gov.tno.nlp.events.ContentReadyEvent;
import ca.bc.gov.tno.nlp.events.ErrorEvent;
import ca.bc.gov.tno.nlp.models.SourceContent;
import ca.bc.gov.tno.nlp.models.Tag;
import ca.bc.gov.tno.nlp.models.TagKey;
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

  @Autowired
  private ApplicationEventPublisher applicationEventPublisher;

  /**
   * Parse content and extract keywords, Name Entity Recognition (NER).
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
      applicationEventPublisher.publishEvent(readyEvent);
    } catch (IOException ex) {
      // TODO: Failed content needs to identified so that it can be rerun. Or it needs
      // to be pushed as it is into the next queue.
      var errorEvent = new ErrorEvent(this, ex);
      applicationEventPublisher.publishEvent(errorEvent);
    }
  }

  /**
   * Perform each NLP action and update the content.
   * 
   * @param content
   * @throws IOException
   */
  private void process(SourceContent content, List<Tag> tags) throws IOException {
    SimpleTokenizer tokenizer = SimpleTokenizer.INSTANCE;
    String[] tokens = tokenizer.tokenize(content.getBody());

    var language = content.getLanguage() == null || content.getLanguage().trim().isEmpty() ? "en"
        : content.getLanguage().trim().toLowerCase();
    content.setLanguage(language);

    // TODO: Make async
    addTags(tags, TagKey.person, extractNER(tokens, String.format("/models/%s-ner-person.bin", language)));
    addTags(tags, TagKey.organization, extractNER(tokens, String.format("/models/%s-ner-organization.bin", language)));
    addTags(tags, TagKey.location, extractNER(tokens, String.format("/models/%s-ner-location.bin", language)));
    addTags(tags, TagKey.date, extractNER(tokens, String.format("/models/%s-ner-date.bin", language)));

    // TODO: extract keywords and phrases.
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
