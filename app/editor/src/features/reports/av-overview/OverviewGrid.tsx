import { useFormikContext } from 'formik';
import moment from 'moment';
import React from 'react';
import { DragDropContext, Draggable, Droppable } from 'react-beautiful-dnd';
import { FaGripLines, FaTrash } from 'react-icons/fa';
import { useContent } from 'store/hooks';
import {
  AVOverviewItemTypeName,
  castEnumToOptions,
  Col,
  FieldSize,
  FormikSelect,
  FormikTextArea,
  FormikTimeInput,
  IAVOverviewInstanceModel,
  IOptionItem,
  OptionItem,
  Row,
  Show,
} from 'tno-core';

import * as styled from './styled';

export interface IOverviewGridProps {
  /** whether line item is editable or not  */
  editable: boolean;
  /** index for av overview item */
  index: number;
}

/** OverviewGrid contains the table of items displayed for each overview section. */
export const OverviewGrid: React.FC<IOverviewGridProps> = ({ editable = true, index }) => {
  const { values, setFieldValue } = useFormikContext<IAVOverviewInstanceModel>();
  const [, { findContent }] = useContent();

  const [clips, setClips] = React.useState<IOptionItem[]>();
  const eveningOverviewItemTypeOptions = castEnumToOptions(AVOverviewItemTypeName);
  const items = values.sections[index].items;
  const startTime = values.sections[index]?.startTime?.split(':');

  /** flag to keep track of when new complete start time is entered and trigger another search
   * for relevant clips
   */
  const shouldFetch = React.useMemo(() => {
    return !values.sections[index].startTime.includes('_') && !!values.sections[index].startTime;
  }, [index, values.sections]);

  /** fetch pieces of content that are related to the series to display as options for associated clips, search for clips published after the start time if it is specified - otherwise filter based on that day.*/
  React.useEffect(() => {
    if (shouldFetch) {
      findContent({
        seriesId: values.sections[index].seriesId,
        publishedStartOn: !!values.sections[index].startTime
          ? moment()
              .set({
                hour: Number(startTime[0]),
                minute: Number(startTime[1]),
                second: Number(startTime[2]),
                millisecond: 0,
              })
              .toISOString()
          : moment().startOf('day').toISOString(),
        contentTypes: [],
      }).then((data) =>
        setClips(data.items.map((c) => new OptionItem(c.headline, c.id)) as IOptionItem[]),
      );
    }
    // only want to fire on init, and when start time is changed
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [shouldFetch]);

  /** function that runs after a user drops an item in the list */
  const handleDrop = (droppedItem: any) => {
    if (!droppedItem.destination) {
      return;
    }
    var updatedList = [...items];
    // Remove dragged item
    const [reorderedItem] = updatedList.splice(droppedItem.source.index, 1);
    // Add dropped item
    updatedList.splice(droppedItem.destination.index, 0, reorderedItem);
    // Update State
    setFieldValue(
      `sections.${index}.items`,
      updatedList.map((item, index) => ({ ...item, sortOrder: index })),
    );
  };

  const handleDeleteItem = (itemIndex: number) => {
    const rows = [...items];
    rows.splice(itemIndex, 1);
    setFieldValue(
      `sections.${index}.items`,
      rows.map((item, index) => ({ ...item, sortOrder: index })),
    );
  };

  return (
    <styled.OverviewGrid>
      <Show visible={!items.length}>
        <div className="no-items">
          There are no items in this section, add a source then click "New story" to begin.
        </div>
      </Show>
      <Show visible={!!items.length}>
        <div className="grid">
          <Row className="header">
            <Col className="placement-header">Placement</Col>
            <Col className="time-header">Time</Col>
            <Col className="summary-header" flex="1">
              Summary
            </Col>
            <Col className="content-header">Clip</Col>
            <Col className="delete-header"></Col>
          </Row>
          <div className="contents">
            <DragDropContext onDragEnd={handleDrop}>
              <Droppable droppableId="list-container" isDropDisabled={!editable}>
                {(provided) => (
                  <div
                    className="list-container"
                    {...provided.droppableProps}
                    ref={provided.innerRef}
                  >
                    {items.map((item, itemIndex) => (
                      <Draggable
                        key={itemIndex}
                        draggableId={itemIndex.toString()}
                        index={itemIndex}
                      >
                        {(provided) => (
                          <div
                            className="item-container"
                            ref={provided.innerRef}
                            key={itemIndex + `item.id`}
                            {...provided.dragHandleProps}
                            {...provided.draggableProps}
                          >
                            <Row className="rows" key={itemIndex} nowrap>
                              <FaGripLines className="grip-lines" />
                              <FormikSelect
                                key={itemIndex + `select`}
                                name={`sections.${index}.items.${itemIndex}.itemType`}
                                width={FieldSize.Small}
                                options={eveningOverviewItemTypeOptions}
                                value={eveningOverviewItemTypeOptions.find(
                                  (o) => o.value === item.itemType,
                                )}
                                isClearable={false}
                                isDisabled={!editable}
                              />
                              <FormikTimeInput
                                key={itemIndex}
                                name={`sections.${index}.items.${itemIndex}.time`}
                                width={FieldSize.Small}
                                placeholder="hh:mm:ss"
                                disabled={!editable}
                              />
                              <Col flex="1">
                                <FormikTextArea
                                  key={itemIndex + `textarea`}
                                  name={`sections.${index}.items.${itemIndex}.summary`}
                                  rows={item.itemType === AVOverviewItemTypeName.Intro ? 3 : 1}
                                  disabled={!editable}
                                  maxLength={
                                    item.itemType === AVOverviewItemTypeName.Story ? 80 : undefined
                                  }
                                />
                              </Col>
                              <FormikSelect
                                name={`sections.${index}.items.${itemIndex}.contentId`}
                                value={clips?.find((c) => c.value === item.contentId)}
                                options={clips ?? []}
                                width={FieldSize.Medium}
                                isDisabled={!editable}
                              />
                              <FaTrash
                                className="clear-item"
                                key={itemIndex + `trash`}
                                onClick={() => handleDeleteItem(itemIndex)}
                              />
                            </Row>
                          </div>
                        )}
                      </Draggable>
                    ))}
                    {provided.placeholder}
                  </div>
                )}
              </Droppable>
            </DragDropContext>
          </div>
        </div>
      </Show>
    </styled.OverviewGrid>
  );
};
