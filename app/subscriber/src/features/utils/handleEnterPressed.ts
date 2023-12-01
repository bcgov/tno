import { KeyboardEvent } from 'react';

/**
 * Handles enter key press on an element
 * @param event generic keyboard event
 * @param callback callback function to be called when enter is pressed on the element
 * @param preventDefault if true, prevents default behaviour of the event
 */
export const handleEnterPressed = <T>(
  event: KeyboardEvent<T>,
  callback: () => void,
  preventDefault?: boolean,
) => {
  if (event.key === 'Enter') {
    preventDefault && event.preventDefault();
    callback();
  }
};
