import { ToolBarSection } from 'components/tool-bar';
import { useTooltips } from 'hooks';
import { FaFileAlt, FaFileAudio, FaFileImage } from 'react-icons/fa';
import { GiFairyWand } from 'react-icons/gi';
import { useNavigate } from 'react-router-dom';
/**
 * Section containing the create new content buttons
 * @returns Section with three separate create content buttons
 */
export const CreateNewSection: React.FC = () => {
  useTooltips();
  const navigate = useNavigate();
  return (
    <ToolBarSection
      children={
        <div className="create-new">
          <FaFileAudio
            data-tip="Snippet"
            data-for="main-tooltip"
            onClick={() => navigate('/snippets/0')}
            className="action-button"
          />
          <FaFileAlt
            data-tip="Print content"
            data-for="main-tooltip"
            onClick={() => navigate('/papers/0')}
            className="action-button"
          />
          <FaFileImage
            data-tip="Image"
            data-for="main-tooltip"
            onClick={() => navigate('/images/0')}
            className="action-button"
          />
        </div>
      }
      label="CREATE NEW"
      icon={<GiFairyWand />}
    />
  );
};
