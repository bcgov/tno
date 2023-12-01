import { KeyboardEvent } from 'react';

/**
 * Handles enter key press on an element
 * @param event generic keyboard event
 * @param callback callback function to be called when enter is pressed on the element
 */
export const handleEnterPressed = <T>(event: KeyboardEvent<T>, callback: () => void) => {
  if (event.key === 'Enter') {
    callback();
  }
};
