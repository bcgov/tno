import { useFormikContext } from 'formik';
import React from 'react';
import { FaCalendarTimes } from 'react-icons/fa';
import { useLookup } from 'store/hooks';
import { Row, Show, ToggleGroup, ToolBar, ToolBarSection } from 'tno-core';

import { IContentForm } from '../../../form/interfaces';
import { ActionSection, AlertSection, PublishedSection, StatusSection } from './form';

export interface IContentFormToolBarProps {
  /** Function to fetch content. */
  fetchContent: (id: number) => void;
  /** Root path for combined view. */
  combinedPath?: string;
}

/**
 * Component that displays the content toolbar section.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ContentFormToolBar = React.forwardRef<HTMLDivElement, IContentFormToolBarProps>(
  ({ fetchContent, combinedPath }, ref) => {
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
        <StatusSection values={values} fetchContent={fetchContent} combinedPath={combinedPath} />
        <AlertSection values={values} ref={ref} />
        <ActionSection values={values} />
        <Show visible={width > 1800}>
          <ToolBarSection
            children={
              <ToggleGroup
                defaultSelected={values.licenseId}
                options={licenses.map((l) => ({
                  id: l.id,
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
        <Show visible={values.status === 'Publish' || values.status === 'Published'}>
          <PublishedSection values={values} />
        </Show>
      </ToolBar>
    );
  },
);
