import { Row, ToolBarSection } from 'tno-core';

export interface IStatusSectionProps {
  status?: string;
}

export const StatusSection: React.FC<IStatusSectionProps> = ({ status }) => {
  return (
    <ToolBarSection
      title="Content Details"
      children={
        <Row>
          <div className="white-bg">{status}</div>
        </Row>
      }
    />
  );
};
