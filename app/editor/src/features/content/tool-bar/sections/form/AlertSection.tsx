import { ContentActions } from 'features/content/form';
import { FaPaperPlane } from 'react-icons/fa';
import { ActionName, Row, ToolBarSection } from 'tno-core';

export interface IAlertSectionProps {}

export const AlertSection: React.FC<IAlertSectionProps> = () => {
  return (
    <ToolBarSection
      label="EMAIL ALERT"
      icon={<FaPaperPlane />}
      children={
        <Row>
          <ContentActions filter={(a) => a.name === ActionName.Alert} />
          <div className="white-bg">not sent</div>
        </Row>
      }
    />
  );
};
