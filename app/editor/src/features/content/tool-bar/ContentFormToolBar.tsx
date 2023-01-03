import { ToggleGroup } from 'components/toggle-group';
import { ToolBarSection } from 'components/tool-bar';
import { ToolBar } from 'components/tool-bar/styled';
import { useFormikContext } from 'formik';
import { ContentTypeName, IActionModel } from 'hooks';
import { FaCalendarTimes } from 'react-icons/fa';
import { useLookup } from 'store/hooks';

import { IContentForm } from '../form/interfaces';
import { ActionSection, AlertSection, StatusSection } from './sections/form';

export interface IContentFormToolBarProps {
  contentType: ContentTypeName;
  determineActions: (a: IActionModel) => boolean;
  status: string;
}

export const ContentFormToolBar: React.FC<IContentFormToolBarProps> = ({
  contentType,
  determineActions,
  status,
}) => {
  const [{ licenses }] = useLookup();
  const { setFieldValue, values } = useFormikContext<IContentForm>();
  return (
    <ToolBar variant="dark">
      <StatusSection status={status} />
      <AlertSection />
      <ActionSection contentType={contentType} determineActions={determineActions} />
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
        label="LICENCE EXPIRY"
        icon={<FaCalendarTimes />}
      />
    </ToolBar>
  );
};
