import { useFormikContext } from 'formik';
import moment from 'moment';
import React from 'react';
import {
  DragDropContext,
  Draggable,
  Droppable,
  DropResult,
  ResponderProvided,
} from 'react-beautiful-dnd';
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
  IAVOverviewSectionItemModel,
  IAVOverviewSectionModel,
  IAVOverviewTemplateSectionItemModel,
  IContentModel,
  IOptionItem,
  OptionItem,
  Row,
  Show,
} from 'tno-core';

import * as styled from './styled';
import { SummarySuggestion } from './SummarySuggestion';
import {
  generateElasticsearchQuery,
  generateListOfSummaries,
  ISectionSummary,
  stringifyNumber,
} from './utils';

export interface IOverviewSectionFormProps {
  /** whether line item is editable or not  */
  editable: boolean;
  /** index for av overview section item */
  index: number;
  summaries: ISectionSummary[];
  setSummaries: React.Dispatch<React.SetStateAction<ISectionSummary[]>>;
}

/**
 * OverviewGrid contains the table of items displayed for each overview section.
 * @param param0 Component properties
 * @returns Component
 */
export const OverviewSectionForm: React.FC<IOverviewSectionFormProps> = ({
  editable = true,
  index,
  summaries,
  setSummaries,
}) => {
  const { values, setFieldValue } = useFormikContext<IAVOverviewInstanceModel>();
  const [, { findContentWithElasticsearch }] = useContent();

  const [contentItems, setContentItems] = React.useState<IContentModel[]>();
  const [showAutoCompleteForIndex, setShowAutoCompleteForIndex] = React.useState<number | null>(
    null,
  );
  const [clips, setClips] = React.useState<IOptionItem[]>([]);

  const eveningOverviewItemTypeOptions = castEnumToOptions(AVOverviewItemTypeName);
  const section = values.sections[index];

  /** fetch pieces of content that are related to the series to display as options for associated clips, search for clips published after the start time if it is specified - otherwise filter based on that day.*/
  const findClips = React.useCallback(
    async (startDate: string | Date, startTime: string, seriesId?: number, sourceId?: number) => {
      try {
        const query = generateElasticsearchQuery(
          new Date(startDate),
          startTime,
          seriesId,
          sourceId,
        );
        const data = await findContentWithElasticsearch(query, false);
        const results: IContentModel[] = data.hits.hits.map((h) => h._source!);
        setContentItems(results);
      } catch {}
    },
    [findContentWithElasticsearch],
  );

  const updateClips = React.useCallback(
    (
      sectionIndex: number,
      section: IAVOverviewSectionModel,
      clips: IOptionItem<string | number | undefined>[],
    ) => {
      // check if any previously selected clips are no longer available, if not, unselect them
      var isUpdated = false;
      var sectionItems = section.items.map((item) => {
        if (item.contentId && !clips.some((clip) => clip.value === item.contentId)) {
          isUpdated = true;
          return { ...item, contentId: undefined };
        }
        return item;
      });
      if (isUpdated) {
        setFieldValue(`sections.${sectionIndex}.items`, sectionItems);
      }
    },
    [setFieldValue],
  );

  /** function that runs after a user drops an item in the list */
  const handleDrop = React.useCallback(
    (
      sectionIndex: number,
      items: IAVOverviewSectionItemModel[],
      result: DropResult,
      provided: ResponderProvided,
    ) => {
      if (!result.destination) {
        return;
      }
      var updatedList = [...items];
      // Remove dragged item
      const [reorderedItem] = updatedList.splice(result.source.index, 1);
      // Add dropped item
      updatedList.splice(result.destination.index, 0, reorderedItem);

      const droppedItemValues = items[result.source.index];

      // If an Ad was reordered, update the ad summary texts accordingly
      if (droppedItemValues.itemType === 'Ad') {
        let count = 0;
        updatedList.forEach((item: IAVOverviewTemplateSectionItemModel) => {
          if (item.itemType === 'Ad') {
            count++;
            item.summary = `${stringifyNumber(count)} commercial break`;
          }
        });
      }

      // Update State
      setFieldValue(
        `sections.${sectionIndex}.items`,
        updatedList.map((item, index) => ({ ...item, sortOrder: index })),
      );
    },
    [setFieldValue],
  );

  /** Remove the line item from the section. */
  const handleDeleteItem = React.useCallback(
    (sectionIndex: number, itemIndex: number, items: IAVOverviewSectionItemModel[]) => {
      const rows = [...items];
      rows.splice(itemIndex, 1);
      setFieldValue(
        `sections.${sectionIndex}.items`,
        rows.map((item, index) => ({ ...item, sortOrder: index })),
      );
    },
    [setFieldValue],
  );

  React.useEffect(() => {
    if (values.publishedOn && !!section.startTime && !section.startTime?.includes('_')) {
      findClips(values.publishedOn, section.startTime, section.seriesId, section.sourceId);
    }
    // Only search for clips when key fields change.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [values.publishedOn, section.startTime, section.seriesId, section.sourceId]);

  React.useEffect(() => {
    if (contentItems) {
      const newClips = contentItems.map((c) => {
        const publishedOnTime = c.publishedOn ? `${moment(c.publishedOn).format('HH:mm')} ` : '';
        const itemHeadline = `${publishedOnTime}${c.headline}`;
        return new OptionItem(itemHeadline, c.id);
      }) as IOptionItem[];
      setClips(newClips);
      updateClips(index, section, newClips);
    }
    // Only prepare clip options with the content items are updated.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [contentItems]);

  return (
    <styled.OverviewGrid>
      <Show visible={!section.items.length}>
        <div className="no-items">
          There are no items in this section, add a source then click "New story" to begin.
        </div>
      </Show>
      <Show visible={!!section.items.length}>
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
            <DragDropContext
              onDragEnd={(result, provided) => handleDrop(index, section.items, result, provided)}
            >
              <Droppable droppableId="list-container" isDropDisabled={!editable}>
                {(provided) => (
                  <div
                    className="list-container"
                    {...provided.droppableProps}
                    ref={provided.innerRef}
                  >
                    {section.items.map((item, itemIndex) => {
                      return (
                        <Draggable
                          key={itemIndex}
                          draggableId={itemIndex.toString()}
                          index={itemIndex}
                        >
                          {(provided) => (
                            <div
                              ref={provided.innerRef}
                              key={itemIndex + `item.id`}
                              {...provided.dragHandleProps}
                              {...provided.draggableProps}
                            >
                              <Row className="rows" key={itemIndex} nowrap>
                                <Col style={{ justifyContent: 'center' }}>
                                  <FaGripLines className="grip-lines" />
                                </Col>
                                <Col>
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
                                    onChange={(newValue: unknown) => {
                                      const option = newValue as IOptionItem;
                                      if (option?.value === 'Ad') {
                                        const numberOfExistingAds = section.items.reduce(
                                          (acc: number, curr: IAVOverviewSectionItemModel) => {
                                            if (curr.itemType === 'Ad') {
                                              acc++;
                                            }
                                            return acc;
                                          },
                                          1,
                                        );
                                        setFieldValue(
                                          `sections.${index}.items.${itemIndex}.summary`,
                                          `${stringifyNumber(
                                            numberOfExistingAds,
                                          )} commercial break`,
                                        );
                                      }
                                    }}
                                  />
                                </Col>
                                <Col>
                                  <FormikTimeInput
                                    key={itemIndex}
                                    name={`sections.${index}.items.${itemIndex}.time`}
                                    width={FieldSize.Small}
                                    placeholder="hh:mm:ss"
                                    disabled={!editable}
                                  />
                                </Col>
                                <Col
                                  flex="1 1 40em"
                                  style={{
                                    position: 'relative',
                                  }}
                                >
                                  <div
                                    onClick={() => setShowAutoCompleteForIndex(null)}
                                    style={{
                                      display:
                                        showAutoCompleteForIndex === itemIndex ? 'block' : 'none',
                                      width: '200vw',
                                      height: '200vh',
                                      backgroundColor: 'transparent',
                                      position: 'fixed',
                                      zIndex: 0,
                                      top: 0,
                                      left: 0,
                                    }}
                                  />
                                  <FormikTextArea
                                    name={`sections.${index}.items.${itemIndex}.summary`}
                                    rows={item.itemType === AVOverviewItemTypeName.Intro ? 3 : 1}
                                    disabled={!editable || item.itemType === 'Ad'}
                                    maxLength={
                                      item.itemType === AVOverviewItemTypeName.Intro ? 2000 : 83
                                    }
                                    onFocus={() => {
                                      setShowAutoCompleteForIndex(null);
                                    }}
                                    onChange={(e) => {
                                      setFieldValue(
                                        `sections.${index}.items.${itemIndex}.summary`,
                                        e.target.value,
                                      );
                                      setShowAutoCompleteForIndex(itemIndex);
                                    }}
                                    onBlur={() => {
                                      setSummaries(generateListOfSummaries(values.sections));
                                    }}
                                  />
                                  <SummarySuggestion
                                    sectionIndex={index}
                                    itemIndex={itemIndex}
                                    show={itemIndex === showAutoCompleteForIndex}
                                    suggestions={summaries}
                                    onClose={() => setShowAutoCompleteForIndex(null)}
                                  />
                                </Col>
                                <Col flex="0.1 1 20em">
                                  <FormikSelect
                                    name={`sections.${index}.items.${itemIndex}.contentId`}
                                    value={clips.find((c) => c.value === item.contentId) ?? ''}
                                    options={clips}
                                    isDisabled={!editable}
                                    maxMenuHeight={120}
                                    onChange={(newValue) => {
                                      const option = newValue as OptionItem;
                                      const item = section.items[itemIndex];
                                      if (option && option.value) {
                                        const content = contentItems?.find(
                                          (ci) => ci.id === option.value,
                                        );
                                        setFieldValue(`sections.${index}.items.${itemIndex}`, {
                                          ...item,
                                          contentId: content?.id,
                                          time: content
                                            ? `${moment(content.publishedOn).format('HH:mm:ss')}`
                                            : undefined,
                                        });
                                      } else {
                                        setFieldValue(`sections.${index}.items.${itemIndex}`, {
                                          ...item,
                                          contentId: undefined,
                                          time: section.startTime,
                                        });
                                      }
                                    }}
                                  />
                                </Col>
                                <FaTrash
                                  style={{ flexShrink: 0 }}
                                  className="clear-item"
                                  key={itemIndex + `trash`}
                                  onClick={() => handleDeleteItem(index, itemIndex, section.items)}
                                />
                              </Row>
                            </div>
                          )}
                        </Draggable>
                      );
                    })}
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
