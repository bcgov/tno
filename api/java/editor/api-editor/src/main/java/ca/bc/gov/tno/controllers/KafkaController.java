package ca.bc.gov.tno.controllers;

import java.io.IOException;
import java.time.Duration;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Properties;
import java.util.Set;
import java.util.concurrent.ExecutionException;

import org.apache.kafka.clients.admin.KafkaAdminClient;
import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.clients.consumer.KafkaConsumer;
import org.apache.kafka.clients.producer.KafkaProducer;
import org.apache.kafka.clients.producer.ProducerConfig;
import org.apache.kafka.clients.producer.ProducerRecord;
import org.apache.kafka.common.serialization.StringDeserializer;
import org.apache.kafka.common.serialization.StringSerializer;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

/**
 * Endpoints to communicate with Kafka.
 */
@RestController
@RequestMapping("/kafka")
public class KafkaController {
  @Value("${kafka.bootstrap.servers}")
  String bootstrapServers;

  /**
   * List all topics
   *
   * @return
   * @throws InterruptedException
   * @throws ExecutionException
   */
  @GetMapping("/topics")
  public Set<String> getTopics() throws InterruptedException, ExecutionException {
    var props = new Properties();
    props.put("bootstrap.servers", bootstrapServers);
    props.put("connections.max.idle.ms", 10000);
    props.put("request.timeout.ms", 5000);

    try (var client = KafkaAdminClient.create(props)) {
      var topics = client.listTopics();
      var names = topics.names().get();
      if (names.isEmpty()) {
        // case: if no topic found.
      }
      return names;
    } catch (InterruptedException | ExecutionException e) {
      // Kafka is not available
      throw e;
    }
  }

  /**
   * Push to a Kafka topic.
   *
   * @return
   * @throws ExecutionException
   * @throws InterruptedException
   * @throws IOException
   */
  @PostMapping(path = "/topics/{topic}/{key}", consumes = { MediaType.APPLICATION_JSON_VALUE })
  public Map<String, Object> pushMessage(@PathVariable String topic, @PathVariable String key,
      @RequestBody String value)
      throws InterruptedException, ExecutionException {
    var props = new Properties();
    props.put(ProducerConfig.CLIENT_ID_CONFIG, "rss-01");
    props.put(ProducerConfig.BOOTSTRAP_SERVERS_CONFIG, bootstrapServers);
    props.put(ProducerConfig.ACKS_CONFIG, "all");
    props.put(ProducerConfig.ENABLE_IDEMPOTENCE_CONFIG, true);
    props.put(ProducerConfig.MAX_IN_FLIGHT_REQUESTS_PER_CONNECTION, 5);
    props.put(ProducerConfig.RETRIES_CONFIG, 10000000);
    props.put(ProducerConfig.BATCH_SIZE_CONFIG, 16384);
    props.put(ProducerConfig.LINGER_MS_CONFIG, 1);
    props.put(ProducerConfig.BUFFER_MEMORY_CONFIG, 33554432);
    props.put(ProducerConfig.KEY_SERIALIZER_CLASS_CONFIG, StringSerializer.class.getName());
    props.put(ProducerConfig.VALUE_SERIALIZER_CLASS_CONFIG, StringSerializer.class.getName());

    var producer = new KafkaProducer<String, String>(props);
    var response = producer.send(new ProducerRecord<String, String>(topic, key, value));
    var result = response.get();
    producer.close();

    var model = new HashMap<String, Object>();
    model.put("offset", result.offset());
    model.put("timestamp", result.timestamp());
    model.put("topic", result.topic());
    model.put("partition", result.partition());
    model.put("serializedKeySize", result.serializedKeySize());
    model.put("serializedValueSize", result.serializedValueSize());
    return model;
  }

  /**
   * Fetch all the messages in the specified topic.
   *
   * @param topic
   * @return
   */
  @GetMapping(path = "/topics/{topic}")
  public List<Map<String, String>> getTopic(@PathVariable String topic) {
    var props = new Properties();
    props.put(ConsumerConfig.CLIENT_ID_CONFIG, "rss-01");
    props.put(ConsumerConfig.BOOTSTRAP_SERVERS_CONFIG, bootstrapServers);
    props.put(ConsumerConfig.GROUP_ID_CONFIG, "test");
    props.put(ConsumerConfig.ENABLE_AUTO_COMMIT_CONFIG, "false");
    props.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest");
    props.put(ConsumerConfig.KEY_DESERIALIZER_CLASS_CONFIG, StringDeserializer.class.getName());
    props.put(ConsumerConfig.VALUE_DESERIALIZER_CLASS_CONFIG, StringDeserializer.class.getName());

    var consumer = new KafkaConsumer<String, String>(props);
    consumer.subscribe(Arrays.asList(topic));

    consumer.seekToBeginning(consumer.assignment());
    var records = consumer.poll(Duration.ofMillis(1000));
    consumer.close();
    List<Map<String, String>> messages = new ArrayList<Map<String, String>>();
    records.records(topic).iterator().forEachRemaining(m -> {
      var record = new HashMap<String, String>();
      var key = m.key() == null ? "NA" : m.key();
      record.put(key, m.value());
      messages.add(record);
    });
    return messages;
  }

}
