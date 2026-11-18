import React from 'react';

import { ChannelMessage } from './ChannelMessage';
import { useChannelEventListener } from './useChannelEventListener';

export interface IChannelProps<T> {
  /** The name of the broadcasting channel. */
  channel?: string;
  /** Callback function when a message is received. */
  onMessage?: (event: MessageEvent<ChannelMessage<T>>) => void;
  /** Callback function when an error is received. */
  onMessageError?: (event: MessageEvent<ChannelMessage<T>>) => void;
}

const defaultName = 'multi-tab-channel';

const defaultConfig: IChannelProps<any> = {
  channel: defaultName,
};

/**
 * Initializes a BroadcastChannel to communicate with multiple tabs.
 * This provides a way to communicate between tabs.
 * @param param0 Hook configuration properties.
 * @returns Function to send messages to the channel.
 */
export const useChannel = <T>({
  channel: initChannel = defaultName,
  onMessage,
  onMessageError,
}: IChannelProps<T> = defaultConfig) => {
  const channelRef = React.useRef(
    typeof window !== 'undefined' && 'BroadcastChannel' in window
      ? new BroadcastChannel(initChannel)
      : null,
  );

  useChannelEventListener(channelRef, 'message', onMessage);
  useChannelEventListener(channelRef, 'messageerror', onMessageError);

  return React.useCallback(
    (type: string, data: T) => channelRef?.current?.postMessage(new ChannelMessage<T>(type, data)),
    [channelRef],
  );
};
