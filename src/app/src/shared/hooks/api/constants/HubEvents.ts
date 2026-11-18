export enum HubEvents {
  /// <summary>
  /// Called when a connection is started.
  /// </summary>
  Connected = 0,
  /// <summary>
  /// Called when a connection is finished.
  /// </summary>
  Disconnected = 1,
  /// <summary>
  /// Sends an invocation message to the specified connection.
  /// </summary>
  SendConnection = 2,
  /// <summary>
  /// Sends an invocation message to the specified connections.
  /// </summary>
  SendConnections = 3,
  /// <summary>
  /// Sends an invocation message to all hub connections.
  /// </summary>
  SendAll = 4,
  /// <summary>
  /// Sends an invocation message to all hub connections excluding the specified connections.
  /// </summary>
  SendAllExcept = 5,
  /// <summary>
  /// Adds a connection to the specified group.
  /// </summary>
  AddToGroup = 6,
  /// <summary>
  /// Removes a connection from the specified group.
  /// </summary>
  RemoveFromGroup = 7,
  /// <summary>
  /// Sends an invocation message to the specified group.
  /// </summary>
  SendGroup = 8,
  /// <summary>
  /// Sends an invocation message to the specified group excluding the specified connections.
  /// </summary>
  SendGroupExcept = 9,
  /// <summary>
  /// Sends an invocation message to the specified groups.
  /// </summary>
  SendGroups = 10,
  /// <summary>
  /// Sends an invocation message to the specified user.
  /// </summary>
  SendUser = 11,
  /// <summary>
  /// Sends an invocation message to the specified users.
  /// </summary>
  SendUsers = 12,
}
