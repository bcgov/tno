import { ContentActions } from 'features/content/form';
import { IContentForm } from 'features/content/form/interfaces';
import React from 'react';
import { FaHighlighter } from 'react-icons/fa';
import { useLookup } from 'store/hooks';
import { Row, Settings, ToolBarSection } from 'tno-core';

export interface IActionSectionProps {
  /** Form values. */
  values: IContentForm;
}

/**
 * Component to display actions for the specified content.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ActionSection: React.FC<IActionSectionProps> = ({ values }) => {
  const [{ settings }] = useLookup();

  const alertId = settings.find((s) => s.name === Settings.DefaultAlert)?.value ?? '0';

  return (
    <ToolBarSection
      label="HIGHLIGHT STORY"
      icon={<FaHighlighter />}
      style={{ maxWidth: '50%' }}
      children={
        <Row gap="0.5rem" alignItems="center" justifyContent="center">
          <ContentActions
            init
            filter={(a) => a.id !== +alertId && a.contentTypes?.includes(values.contentType)}
          />
        </Row>
      }
    />
  );
};
