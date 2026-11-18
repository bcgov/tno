export enum HubEventsName {
  /// <summary>
  /// Called when a connection is started.
  /// </summary>
  Connected = 'Connected',
  /// <summary>
  /// Called when a connection is finished.
  /// </summary>
  Disconnected = 'Disconnected',
  /// <summary>
  /// Sends an invocation message to the specified connection.
  /// </summary>
  SendConnection = 'SendConnection',
  /// <summary>
  /// Sends an invocation message to the specified connections.
  /// </summary>
  SendConnections = 'SendConnections',
  /// <summary>
  /// Sends an invocation message to all hub connections.
  /// </summary>
  SendAll = 'SendAll',
  /// <summary>
  /// Sends an invocation message to all hub connections excluding the specified connections.
  /// </summary>
  SendAllExcept = 'SendAllExcept',
  /// <summary>
  /// Adds a connection to the specified group.
  /// </summary>
  AddToGroup = 'AddToGroup',
  /// <summary>
  /// Removes a connection from the specified group.
  /// </summary>
  RemoveFromGroup = 'RemoveFromGroup',
  /// <summary>
  /// Sends an invocation message to the specified group.
  /// </summary>
  SendGroup = 'SendGroup',
  /// <summary>
  /// Sends an invocation message to the specified group excluding the specified connections.
  /// </summary>
  SendGroupExcept = 'SendGroupExcept',
  /// <summary>
  /// Sends an invocation message to the specified groups.
  /// </summary>
  SendGroups = 'SendGroups',
  /// <summary>
  /// Sends an invocation message to the specified user.
  /// </summary>
  SendUser = 'SendUser',
  /// <summary>
  /// Sends an invocation message to the specified users.
  /// </summary>
  SendUsers = 'SendUsers',
}
