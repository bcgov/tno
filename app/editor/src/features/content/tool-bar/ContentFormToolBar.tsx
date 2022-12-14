import { ToolBar } from 'components/tool-bar/styled';
import { ContentTypeName, IActionModel } from 'hooks';

import { ActionSection, AlertSection } from './sections/form';

// import { ActionSection } from './sections/form';

export interface IContentFormToolBarProps {
  contentType: ContentTypeName;
  determineActions: (a: IActionModel) => boolean;
}

export const ContentFormToolBar: React.FC<IContentFormToolBarProps> = ({
  contentType,
  determineActions,
}) => {
  return (
    <ToolBar className="dark-bg">
      <ActionSection contentType={contentType} determineActions={determineActions} />
      <AlertSection />
    </ToolBar>
  );
};
