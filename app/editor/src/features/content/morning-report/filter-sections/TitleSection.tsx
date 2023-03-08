import { ToolBarSection } from 'components/tool-bar';
import { FaEye, FaSun } from 'react-icons/fa';
import { Col } from 'tno-core';

export interface ITitleSectionProps {}

export const TitleSection: React.FC<ITitleSectionProps> = ({}) => {
  return (
    <ToolBarSection
      className="morning-report-title"
      children={
        <Col>
          <p className="report-title">Morning Report</p>
          <FaSun className="title-icon" />
        </Col>
      }
      label="VIEWING"
      icon={<FaEye />}
    />
  );
};
