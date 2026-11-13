/**
 * A wrapper for channel messages so that an 'type' identifier can be included.
 */
export class ChannelMessage<T> {
  type: string;
  message: T;

  constructor(type: string, message: T) {
    this.type = type;
    this.message = message;
  }
}
