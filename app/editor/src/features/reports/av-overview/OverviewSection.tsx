import { useFormikContext } from 'formik';
import React from 'react';
import { FaTrash } from 'react-icons/fa';
import { MdAdd, MdClear } from 'react-icons/md';
import {
  AVOverviewItemTypeName,
  Button,
  ButtonVariant,
  IAVOverviewInstanceModel,
  Row,
  Show,
} from 'tno-core';

import { defaultAVOverviewSectionItem } from './constants';
import { EditBroadcastDetails } from './EditBroadcastDetails';
import { OverviewGrid } from './OverviewGrid';
import * as styled from './styled';

export interface IOverviewSectionProps {
  editable?: boolean;
  index: number;
}

/** This section includes the overview grid, as well as the broadcast details. */
export const OverviewSection: React.FC<IOverviewSectionProps> = ({ editable = true, index }) => {
  const { values, setFieldValue } = useFormikContext<IAVOverviewInstanceModel>();

  const section = values.sections[index];

  const handleDeleteSection = (index: number) => {
    values.sections.splice(index, 1);
    setFieldValue(`sections`, values.sections);
  };

  const handleAddItem = (index: number) => {
    setFieldValue(`sections.${index}.items`, [
      ...section.items,
      defaultAVOverviewSectionItem(
        section.id,
        !section.items.length ? AVOverviewItemTypeName.Intro : AVOverviewItemTypeName.Story,
        section.startTime,
        section.items.length,
      ),
    ]);
  };

  const handleClearItems = (index: number) => {
    setFieldValue(
      `sections.${index}.items`,
      section.items.map((i) => ({ ...i, summary: '' })),
    );
  };

  return (
    <styled.OverviewSection className="section">
      <EditBroadcastDetails
        index={index}
        editable={editable}
        open={!section.sourceId && !section.otherSource && !section.seriesId}
      />
      <OverviewGrid index={index} editable={editable} />
      <Show visible={editable}>
        <Row className="buttons">
          <Row flex="1">
            <Button
              disabled={!section.name}
              variant={ButtonVariant.action}
              onClick={() => handleAddItem(index)}
            >
              New story <MdAdd className="icon" />
            </Button>
            <Button
              variant={ButtonVariant.danger}
              onClick={() => handleClearItems(index)}
              disabled={!section.items.length}
            >
              Clear all story text <MdClear className="icon" />
            </Button>
          </Row>
          <Button
            variant={ButtonVariant.danger}
            onClick={() => handleDeleteSection(index)}
            tooltip={
              !!section.items.length ? 'Delete all items before deleting section' : undefined
            }
            disabled={!!section.items.length}
          >
            Delete section
            <FaTrash className="delete" />
          </Button>
        </Row>
      </Show>
    </styled.OverviewSection>
  );
};
