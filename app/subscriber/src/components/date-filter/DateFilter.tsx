import { FaAngleLeft, FaAngleRight, FaCalendarAlt } from 'react-icons/fa';
import * as styled from './styled';

export const DateFilter: React.FC = () => {
  return (
    <styled.DateFilter justifyContent="center" className="date-navigator">
      <FaAngleLeft />
      2023-03-04
      <FaAngleRight />
      <FaCalendarAlt className="calendar" />
    </styled.DateFilter>
  );
};
