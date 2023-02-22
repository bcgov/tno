using System.Text.Json;
using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Extensions;
using TNO.Kafka.Serializers;

namespace TNO.Kafka.SignalR;

/// <summary>
/// KafkaHubLifetimeManager class, provides a Kafka back-plane to support clusters.
/// All messages are routed to Kafka.
/// This hub is a Kafka consumer and will action the message once it consumes it.
/// </summary>
/// <typeparam name="THub"></typeparam>
public class KafkaHubLifetimeManager<THub> : DefaultHubLifetimeManager<THub>, IDisposable
    where THub : Hub
{
    #region Variables
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly KafkaHubConfig _kafkaConfig;
    private readonly JsonSerializerOptions _serializerConfig;
    private readonly ILogger _logger;
    private bool disposed = false;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Task? _consumingTask;
    #endregion

    #region Properties
    /// <summary>
    /// get - The Kafka consumer.
    /// </summary>
    protected IConsumer<string, KafkaHubMessage> Consumer { get; private set; }

    /// <summary>
    /// get - The Kafka producer.
    /// </summary>
    protected IProducer<string, KafkaHubMessage> Producer { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a KafkaHubLifetimeManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    public KafkaHubLifetimeManager(
        IHttpContextAccessor httpContextAccessor,
        IOptions<KafkaHubConfig> kafkaOptions,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<KafkaHubLifetimeManager<THub>> logger)
        : this(httpContextAccessor, kafkaOptions, null, serializerOptions, logger)
    {
    }

    /// <summary>
    /// Creates a new instance of a KafkaHubLifetimeManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="hubOptions"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    public KafkaHubLifetimeManager(
        IHttpContextAccessor httpContextAccessor,
        IOptions<KafkaHubConfig> kafkaOptions,
        IOptions<HubOptions<THub>>? hubOptions,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<KafkaHubLifetimeManager<THub>> logger)
        : base(logger)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _httpContextAccessor = httpContextAccessor;
        _serializerConfig = serializerOptions.Value;
        _kafkaConfig = kafkaOptions.Value;
        _logger = logger;
        this.Consumer = BuildConsumer();
        this.Consumer.Subscribe(_kafkaConfig.HubTopic);
        StartConsuming(_cancellationTokenSource.Token);

        this.Producer = BuildProducer();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Dispose KafkaHubLifetimeManager.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose KafkaHubLifetimeManager.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                this.Consumer.Unsubscribe();
                if (_consumingTask != null)
                {
                    _cancellationTokenSource.Cancel();
                    if (_consumingTask.IsCompleted)
                        _consumingTask.Dispose();
                }
                _cancellationTokenSource.Dispose();
                this.Consumer.Dispose();
            }
            disposed = true;
        }
    }

    /// <summary>
    /// Create a Kafka key for each message.
    /// Default is the user's email address with a prefix.
    /// </summary>
    /// <param name="prefix"></param>
    /// <returns></returns>
    protected string CreateKey(string prefix)
    {
        return $"{prefix}-{_httpContextAccessor.HttpContext?.User.GetEmail() ?? "anonymous"}";
    }

    /// <summary>
    /// Creates a new Kafka message containing the hub event message.
    /// </summary>
    /// <param name="hubEvent"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <param name="excludedConnectionIds"></param>
    /// <returns></returns>
    protected Message<string, KafkaHubMessage> CreateMessage(HubEvent hubEvent, string methodName, object?[] args, IReadOnlyList<string>? excludedConnectionIds = null)
    {
        return CreateMessage(hubEvent, "", methodName, args, excludedConnectionIds);
    }

    /// <summary>
    /// Creates a new Kafka message containing the hub event message.
    /// </summary>
    /// <param name="hubEvent"></param>
    /// <param name="groupName"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <param name="excludedConnectionIds"></param>
    /// <returns></returns>
    protected Message<string, KafkaHubMessage> CreateMessage(HubEvent hubEvent, string groupName, string methodName, object?[] args, IReadOnlyList<string>? excludedConnectionIds = null)
    {
        return CreateMessage(hubEvent, new String[] { groupName }, methodName, args, excludedConnectionIds);
    }

    /// <summary>
    /// Creates a new Kafka message containing the hub event message.
    /// </summary>
    /// <param name="hubEvent"></param>
    /// <param name="groupNames"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <param name="excludedConnectionIds"></param>
    /// <returns></returns>
    protected Message<string, KafkaHubMessage> CreateMessage(HubEvent hubEvent, IEnumerable<string> groupNames, string methodName, object?[] args, IReadOnlyList<string>? excludedConnectionIds = null)
    {
        return new Message<string, KafkaHubMessage>()
        {
            Key = CreateKey(methodName),
            Value = new KafkaHubMessage(hubEvent, groupNames, new InvocationMessage(methodName, args), excludedConnectionIds)
        };
    }

    #region Hub Event Methods
    /// <summary>
    /// Invoke the connection for the specified 'connectionId' and 'methodName'.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connectionId"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<T> InvokeConnectionAsync<T>(string connectionId, string methodName, object?[] args, CancellationToken cancellationToken)
    {
        return base.InvokeConnectionAsync<T>(connectionId, methodName, args, cancellationToken);
    }

    /// <summary>
    /// On connected inform the hub.
    /// </summary>
    /// <param name="connection"></param>
    /// <returns></returns>
    public override Task OnConnectedAsync(HubConnectionContext connection)
    {
        return base.OnConnectedAsync(connection);
    }

    /// <summary>
    /// On disconnected inform the hub.
    /// </summary>
    /// <param name="connection"></param>
    /// <returns></returns>
    public override Task OnDisconnectedAsync(HubConnectionContext connection)
    {
        return base.OnDisconnectedAsync(connection);
    }

    /// <summary>
    /// Set the connection results for the specified 'connectionId'.
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public override Task SetConnectionResultAsync(string connectionId, CompletionMessage result)
    {
        return base.SetConnectionResultAsync(connectionId, result);
    }

    /// <summary>
    /// Send a connection message for the specified 'connectionId' and 'methodName'.
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task SendConnectionAsync(string connectionId, string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        return base.SendConnectionAsync(connectionId, methodName, args, cancellationToken);
    }

    /// <summary>
    /// Send a connection message for all the specified 'connectionIds' and 'methodName'.
    /// </summary>
    /// <param name="connectionIds"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task SendConnectionsAsync(IReadOnlyList<string> connectionIds, string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        return base.SendConnectionsAsync(connectionIds, methodName, args, cancellationToken);
    }

    /// <summary>
    /// Send message to Kafka back-plane.
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task SendAllAsync(string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        var message = CreateMessage(HubEvent.SendAll, methodName, args);
        await this.Producer.ProduceAsync(_kafkaConfig.HubTopic, message, cancellationToken);
    }

    /// <summary>
    /// Send message to Kafka back-plane.
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <param name="excludedConnectionIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task SendAllExceptAsync(string methodName, object?[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
    {
        var message = CreateMessage(HubEvent.SendAllExcept, methodName, args, excludedConnectionIds);
        await this.Producer.ProduceAsync(_kafkaConfig.HubTopic, message, cancellationToken);
    }

    /// <summary>
    /// Add specified 'connectionId' to the 'groupName'.
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="groupName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
    {
        return base.AddToGroupAsync(connectionId, groupName, cancellationToken);
    }

    /// <summary>
    /// Remove the specified 'connectionId' from the 'groupName'.
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="groupName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
    {
        return base.RemoveFromGroupAsync(connectionId, groupName, cancellationToken);
    }

    /// <summary>
    /// Send message to Kafka back-plane.
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task SendGroupAsync(string groupName, string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        var message = CreateMessage(HubEvent.SendGroup, groupName, methodName, args);
        await this.Producer.ProduceAsync(_kafkaConfig.HubTopic, message, cancellationToken);
    }

    /// <summary>
    /// Send message to Kafka back-plane.
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <param name="excludedConnectionIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task SendGroupExceptAsync(string groupName, string methodName, object?[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
    {
        var message = CreateMessage(HubEvent.SendGroupExcept, groupName, methodName, args, excludedConnectionIds);
        await this.Producer.ProduceAsync(_kafkaConfig.HubTopic, message, cancellationToken);
    }

    /// <summary>
    /// Send message to Kafka back-plane.
    /// </summary>
    /// <param name="groupNames"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task SendGroupsAsync(IReadOnlyList<string> groupNames, string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        var message = CreateMessage(HubEvent.SendGroups, groupNames, methodName, args, null);
        await this.Producer.ProduceAsync(_kafkaConfig.HubTopic, message, cancellationToken);
    }

    /// <summary>
    /// Send message to Kafka back-plane.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task SendUserAsync(string userId, string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        var message = CreateMessage(HubEvent.SendUser, userId, methodName, args);
        await this.Producer.ProduceAsync(_kafkaConfig.HubTopic, message, cancellationToken);
    }

    /// <summary>
    /// Send message to Kafka back-plane.
    /// </summary>
    /// <param name="userIds"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task SendUsersAsync(IReadOnlyList<string> userIds, string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        var message = CreateMessage(HubEvent.SendUsers, userIds, methodName, args, null);
        await this.Producer.ProduceAsync(_kafkaConfig.HubTopic, message, cancellationToken);
    }
    #endregion

    #region Kafka Producer Methods
    /// <summary>
    /// Build a Kafka producer with configured options.
    /// </summary>
    /// <returns></returns>
    protected IProducer<string, KafkaHubMessage> BuildProducer()
    {
        var builder = new ProducerBuilder<string, KafkaHubMessage>(_kafkaConfig.Producer);
        builder.SetKeySerializer(new DefaultJsonSerializer<string>(_serializerConfig));
        builder.SetValueSerializer(new DefaultJsonSerializer<KafkaHubMessage>(_serializerConfig));
        return builder.Build();
    }
    #endregion

    #region Kafka Consumer Methods
    /// <summary>
    /// Build a Kafka consumer with configured options.
    /// </summary>
    /// <returns></returns>
    protected IConsumer<string, KafkaHubMessage> BuildConsumer()
    {
        var builder = new ConsumerBuilder<string, KafkaHubMessage>(_kafkaConfig.Consumer);
        builder.SetKeyDeserializer(new DefaultJsonSerializer<string>(_serializerConfig));
        builder.SetValueDeserializer(new DefaultJsonSerializer<KafkaHubMessage>(_serializerConfig));
        return builder.Build();
    }

    /// <summary>
    /// Start consuming messages from Kafka.
    /// </summary>
    /// <param name="cancellationToken"></param>
    private void StartConsuming(CancellationToken cancellationToken)
    {
        _consumingTask = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = this.Consumer.Consume(cancellationToken);
                    if (result == null)
                        continue;

                    await ConsumeMessageAsync(result, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kafka consumer failed");
                    await Task.Delay(_kafkaConfig.ConsumerExceptionDelayMs);
                }
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Handle messages received from Kafka consumer.
    /// Pass all messages to inherited Hub message handle so that each instance of the hub will communicate to users connected to it.
    /// </summary>
    /// <param name="consumeResult"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task ConsumeMessageAsync(ConsumeResult<string, KafkaHubMessage> consumeResult, CancellationToken cancellationToken)
    {
        var model = consumeResult.Message.Value;
        var message = model.Message;
        _logger.LogDebug("Message received for {event}:{target}", model.HubEvent, message.Target);
        switch (model.HubEvent)
        {
            case HubEvent.SendAll:
                await base.SendAllAsync(message.Target, message.Arguments, cancellationToken);
                break;
            case HubEvent.SendAllExcept:
                await base.SendAllExceptAsync(
                    message.Target,
                    message.Arguments,
                    model.ExcludedConnectionIds ?? throw new InvalidOperationException("Message missing excluded connection ids."),
                    cancellationToken);
                break;
            case HubEvent.SendGroup:
                await base.SendGroupAsync(
                    model.Identifiers?.First() ?? throw new InvalidOperationException("Message missing group name."),
                    message.Target,
                    message.Arguments,
                    cancellationToken);
                break;
            case HubEvent.SendGroupExcept:
                await base.SendGroupExceptAsync(
                    model.Identifiers?.First() ?? throw new InvalidOperationException("Message missing group name."),
                    message.Target,
                    message.Arguments,
                    model.ExcludedConnectionIds ?? throw new InvalidOperationException("Message missing excluded connection ids."),
                    cancellationToken);
                break;
            case HubEvent.SendGroups:
                await base.SendGroupsAsync(
                    model.Identifiers ?? throw new InvalidOperationException("Message missing group names."),
                    message.Target,
                    message.Arguments,
                    cancellationToken);
                break;
            case HubEvent.SendUser:
                await base.SendUserAsync(
                    model.Identifiers?.First() ?? throw new InvalidOperationException("Message missing user id."),
                    message.Target,
                    message.Arguments,
                    cancellationToken);
                break;
            case HubEvent.SendUsers:
                await base.SendUsersAsync(
                    model.Identifiers ?? throw new InvalidOperationException("Message missing user ids."),
                    message.Target,
                    message.Arguments,
                    cancellationToken);
                break;
            default:
                throw new NotImplementedException("Hub event is not implemented.");
        }
    }
    #endregion
    #endregion
}
