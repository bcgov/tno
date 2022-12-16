import { ToolBarSection } from 'components/tool-bar';
import { ContentActions } from 'features/content/form';
import { ContentTypeName, IActionModel, ValueType } from 'hooks';
import { FaHighlighter } from 'react-icons/fa';
import { Row } from 'tno-core';

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
