import React from 'react';

import { ChannelMessage } from './ChannelMessage';

/**
 * Hook provides a way to add and remove a listener to the specified channel.
 * @param channelRef Reference to channel variable.
 * @param event The name of the event to listen to from the BroadcastChannel object.
 * @param listener The callback function that will be executed when an event is fired.
 */
export const useChannelEventListener = <T extends keyof BroadcastChannelEventMap, DT = any>(
  channelRef: React.MutableRefObject<BroadcastChannel | null>,
  event: T,
  listener?: (event: MessageEvent<ChannelMessage<DT>>) => void,
) => {
  React.useEffect(() => {
    const channel = channelRef.current;
    if (channel && listener) {
      channel.addEventListener(event, listener);
      return () => channel.removeEventListener(event, listener);
    }
  }, [channelRef, event, listener]);
};
