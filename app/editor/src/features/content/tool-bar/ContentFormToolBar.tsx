import { ToggleGroup } from 'components/toggle-group';
import { ToolBarSection } from 'components/tool-bar';
import { ToolBar } from 'components/tool-bar/styled';
import { useFormikContext } from 'formik';
import { ContentTypeName, IActionModel } from 'hooks';
import { FaCalendarTimes } from 'react-icons/fa';
import { useLookup } from 'store/hooks';

import { IContentForm } from '../form/interfaces';
import { ActionSection, AlertSection } from './sections/form';

export interface IContentFormToolBarProps {
  contentType: ContentTypeName;
  determineActions: (a: IActionModel) => boolean;
}

export const ContentFormToolBar: React.FC<IContentFormToolBarProps> = ({
  contentType,
  determineActions,
}) => {
  const [{ licenses }] = useLookup();
  const { setFieldValue, values } = useFormikContext<IContentForm>();
  return (
    <ToolBar variant="dark">
      <ActionSection contentType={contentType} determineActions={determineActions} />
      <AlertSection />
      <ToolBarSection
        children={
          <ToggleGroup
            defaultSelected={licenses
              .find((l) => l.id === values.licenseId)
              ?.name.replace(/expire/i, '')
              .toLowerCase()}
            options={licenses.map((l) => ({
              label: l.name.toUpperCase().replace(/expire/i, ''),
              onClick: () => setFieldValue('licenseId', Number(l.id)),
            }))}
          />
        }
        label="LICENSE EXPIRY"
        icon={<FaCalendarTimes />}
      />
    </ToolBar>
  );
};
