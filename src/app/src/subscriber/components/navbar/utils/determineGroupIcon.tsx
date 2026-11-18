import { FaUserAlt } from 'react-icons/fa';
import { FaTableCellsLarge } from 'react-icons/fa6';

// determines the icon associated with a group label on the navbar
export const determineGroupIcon = (groupName: string) => {
  switch (groupName) {
    case 'MY CONTENT':
      return <FaUserAlt />;
    case 'USER RESOURCES':
      return <FaTableCellsLarge />;
    default:
      return null;
  }
};
