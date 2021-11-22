package ca.bc.gov.tno.services.nlp.kafka;

import java.util.Properties;

import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.clients.consumer.KafkaConsumer;
import org.apache.kafka.common.serialization.StringDeserializer;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.kafka.support.serializer.JsonDeserializer;
import org.springframework.kafka.support.serializer.JsonSerializer;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.models.SourceContent;
import ca.bc.gov.tno.models.Tag;
import ca.bc.gov.tno.services.ServiceState;
import ca.bc.gov.tno.services.kafka.BaseKafkaConsumerService;
import ca.bc.gov.tno.services.kafka.config.KafkaConsumerConfig;

@Async
@Component
public class KafkaNlpConsumer extends BaseKafkaConsumerService<String, SourceContent> {

  /**
   * Create a new instance of a KafkaNlpConsumer object. Creates a new instance of
   * a KafkaConsumer that can be used by the run() method.
   */
  @Autowired
  public KafkaNlpConsumer(final ServiceState state, final KafkaConsumerConfig config,
      final ApplicationEventPublisher eventPublisher) {
    super(state, config, eventPublisher);
  }

  /**
   * Initialize the Kafka consumer.
   * 
   * @return A new instance of a KafkaConsumer.
   */
  @Override
  protected KafkaConsumer<String, SourceContent> initConsumer() {
    var props = new Properties();
    props.put(ConsumerConfig.BOOTSTRAP_SERVERS_CONFIG, config.getBootstrapServers());
    props.put(ConsumerConfig.GROUP_ID_CONFIG, config.getGroupId());
    props.put(ConsumerConfig.KEY_DESERIALIZER_CLASS_CONFIG, StringDeserializer.class);
    props.put(ConsumerConfig.VALUE_DESERIALIZER_CLASS_CONFIG, JsonDeserializer.class);
    props.put(JsonSerializer.TYPE_MAPPINGS,
        String.format("Content:%s, Tag:%s", SourceContent.class.getName(), Tag.class.getName()));
    props.put(JsonDeserializer.KEY_DEFAULT_TYPE, String.class.getName());
    props.put(JsonDeserializer.VALUE_DEFAULT_TYPE, SourceContent.class.getName());
    props.put(ConsumerConfig.ENABLE_AUTO_COMMIT_CONFIG, config.isEnableAutoCommit());
    props.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, config.getAutoOffsetRest());
    return new KafkaConsumer<String, SourceContent>(props);
  }
}
