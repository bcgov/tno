import { ContentActions } from 'features/content/form';
import { IContentForm } from 'features/content/form/interfaces';
import { FaPaperPlane } from 'react-icons/fa';
import { ActionName, Row, ToolBarSection } from 'tno-core';

export interface IAlertSectionProps {
  /** Form values. */
  values: IContentForm;
}

export const AlertSection: React.FC<IAlertSectionProps> = ({ values }) => {
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
