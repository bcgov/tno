import { ContentActions } from 'features/content/form';
import { FaHighlighter } from 'react-icons/fa';
import { ContentTypeName, IActionModel, Row, ToolBarSection, ValueType } from 'tno-core';

export interface IActionSectionProps {
  contentType: ContentTypeName;
  determineActions: (a: IActionModel) => boolean;
}

export const ActionSection: React.FC<IActionSectionProps> = ({ contentType, determineActions }) => {
  return (
    <ToolBarSection
      label="HIGHLIGHT STORY"
      icon={<FaHighlighter />}
      children={
        <Row>
          <ContentActions init filter={determineActions} contentType={contentType} />
          <ContentActions filter={(a) => a.valueType !== ValueType.Boolean} />
        </Row>
      }
    />
  );
};
