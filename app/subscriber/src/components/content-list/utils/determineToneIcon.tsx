import { FaFrown, FaMeh, FaSmile } from 'react-icons/fa';
import { IContentTonePoolModel } from 'tno-core';

export const determineToneIcon = (tone?: IContentTonePoolModel) => {
  const value = tone?.value ?? 0;
  if (value > 0) return <FaSmile className="positive tone-icon" />;
  if (value < 0) return <FaFrown className="negative tone-icon" />;
  if (value === 0) return <FaMeh className="neutral tone-icon" />;
};
