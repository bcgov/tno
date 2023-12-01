import { KeyboardEvent } from 'react';

export const handleEnterPressed = (
  event: KeyboardEvent<HTMLInputElement>,
  callback: () => void,
) => {
  if (event.key === 'Enter') {
    callback();
  }
};
