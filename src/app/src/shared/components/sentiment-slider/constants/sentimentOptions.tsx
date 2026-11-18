import { FaRegFrown, FaRegMeh, FaRegSmile } from 'react-icons/fa';

export const sentimentOptions = {
  '-5': <FaRegFrown className="tone-icon" color="#DC3545" />,
  '-4': '-4',
  '-3': '-3',
  '-2': '-2',
  '-1': '-1',
  '0': <FaRegMeh className="tone-icon" color="#FFC107" />,
  '1': '1',
  '2': '2',
  '3': '3',
  '4': '4',
  '5': <FaRegSmile className="tone-icon" color="#20C997" />,
};
