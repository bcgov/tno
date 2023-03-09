import { ToolBarSection } from 'components/tool-bar';
import { FaFileAlt, FaFileAudio, FaFileImage, FaFileInvoice } from 'react-icons/fa';
import { GiFairyWand } from 'react-icons/gi';
import { useNavigate } from 'react-router-dom';
import { Col, Row } from 'tno-core';
/**
 * Section containing the create new content buttons
 * @returns Section with three separate create content buttons
 */
export const CreateNewSection: React.FC = () => {
  const navigate = useNavigate();
  return (
    <ToolBarSection
      children={
        <Col className="create-new">
          <Row>
            <FaFileAudio
              data-tooltip-content="Radio/TV"
              data-tooltip-id="main-tooltip"
              onClick={() => navigate('/snippets/0')}
              className="action-button"
            />
            <FaFileAlt
              data-tooltip-content="Print content"
              data-tooltip-id="main-tooltip"
              onClick={() => navigate('/papers/0')}
              className="action-button"
            />
          </Row>
          <Row>
            <FaFileImage
              data-tooltip-content="Image"
              data-tooltip-id="main-tooltip"
              onClick={() => navigate('/images/0')}
              className="action-button"
            />
            <FaFileInvoice
              data-tooltip-content="Internet"
              data-tooltip-id="main-tooltip"
              onClick={() => navigate('/stories/0')}
              className="action-button"
            />
          </Row>
        </Col>
      }
      label="CREATE SNIPPET"
      icon={<GiFairyWand />}
    />
  );
};
