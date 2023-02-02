import { ToggleGroup } from 'components/toggle-group';
import { ToolBarSection } from 'components/tool-bar';
import { ToolBar } from 'components/tool-bar/styled';
import { useFormikContext } from 'formik';
import { ContentTypeName, IActionModel } from 'hooks';
import React from 'react';
import { FaCalendarTimes } from 'react-icons/fa';
import { useLookup } from 'store/hooks';
import { Row, Show } from 'tno-core';

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
  const [width, setWidth] = React.useState(window.innerWidth);

  // recalculates the width of the screen when the screen is resized
  React.useEffect(() => {
    if (typeof window === 'undefined') {
      return;
    }

    const calcWidth = () => {
      setWidth(window.innerWidth);
    };

    window.addEventListener('resize', calcWidth);

    // This is likely unnecessary, as the initial state should capture
    // the size, however if a resize occurs between initial state set by
    // React and before the event listener is attached, this
    // will just make sure it captures that.
    calcWidth();

    // Return a function to disconnect the event listener
    return () => window.removeEventListener('resize', calcWidth);
  }, []);
  return (
    <ToolBar variant="dark">
      <StatusSection status={status} />
      <AlertSection />
      <ActionSection contentType={contentType} determineActions={determineActions} />
      <Show visible={width > 1800}>
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
      </Show>
      <Show visible={width <= 1800}>
        <ToolBarSection
          children={
            <Row>
              <div className="white-bg">
                {licenses.find((l) => l.id === values.licenseId)?.name}
              </div>
            </Row>
          }
          label="LICENCE EXPIRY"
        />
      </Show>
    </ToolBar>
  );
};
