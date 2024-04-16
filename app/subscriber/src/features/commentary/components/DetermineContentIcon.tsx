import { FaCamera, FaNewspaper, FaPlayCircle, FaTv } from 'react-icons/fa';
import { ContentTypeName } from 'tno-core';

import * as styled from '../components/styled';

export interface IDetermineContentIconProps {
  contentType: ContentTypeName;
}

/** Determine Icon to display based on a commentary item's content type. */
export const DetermineContentIcon: React.FC<IDetermineContentIconProps> = ({ contentType }) => {
  const determineIcon = (contentType: ContentTypeName) => {
    switch (contentType) {
      case ContentTypeName.PrintContent:
        return <FaNewspaper />;
      case ContentTypeName.Internet:
        return <FaTv />;
      case ContentTypeName.AudioVideo:
        return <FaPlayCircle />;
      default:
        return <FaCamera />;
    }
  };
  return <styled.DetermineContentIcon>{determineIcon(contentType)}</styled.DetermineContentIcon>;
};
