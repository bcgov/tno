namespace TNO.Kafka.SignalR;

public enum HubEvent
{
    /// <summary>
    /// Called when a connection is started.
    /// </summary>
    Connected,
    /// <summary>
    /// Called when a connection is finished.
    /// </summary>
    Disconnected,
    /// <summary>
    /// Sends an invocation message to the specified connection.
    /// </summary>
    SendConnection,
    /// <summary>
    /// Sends an invocation message to the specified connections.
    /// </summary>
    SendConnections,
    /// <summary>
    /// Sends an invocation message to all hub connections.
    /// </summary>
    SendAll,
    /// <summary>
    /// Sends an invocation message to all hub connections excluding the specified connections.
    /// </summary>
    SendAllExcept,
    /// <summary>
    /// Adds a connection to the specified group.
    /// </summary>
    AddToGroup,
    /// <summary>
    /// Removes a connection from the specified group.
    /// </summary>
    RemoveFromGroup,
    /// <summary>
    /// Sends an invocation message to the specified group.
    /// </summary>
    SendGroup,
    /// <summary>
    /// Sends an invocation message to the specified group excluding the specified connections.
    /// </summary>
    SendGroupExcept,
    /// <summary>
    /// Sends an invocation message to the specified groups.
    /// </summary>
    SendGroups,
    /// <summary>
    /// Sends an invocation message to the specified user.
    /// </summary>
    SendUser,
    /// <summary>
    /// Sends an invocation message to the specified users.
    /// </summary>
    SendUsers
}
