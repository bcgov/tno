import { FaFileAlt, FaFileAudio, FaFileImage, FaPlusCircle } from 'react-icons/fa';
import { GiFairyWand } from 'react-icons/gi';
import { useNavigate } from 'react-router-dom';

import * as styled from './styled';

/**
 * Section containing the create new content buttons
 * @returns Section with three separate create content buttons
 */
export const CreateNewSection: React.FC = () => {
  const navigate = useNavigate();
  return (
    <styled.CreateNewSection
      actions={
        <div className="create-new">
          <FaFileAudio onClick={() => navigate('/snippets/0')} className="action-button" />
          <FaFileAlt onClick={() => navigate('/papers/0')} className="action-button" />
          <FaFileImage onClick={() => navigate('/images/0')} className="action-button" />
        </div>
      }
      label="CREATE NEW"
      icon={<GiFairyWand />}
    />
  );
};
