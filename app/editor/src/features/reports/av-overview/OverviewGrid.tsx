import { getIn, useFormikContext } from 'formik';
import moment from 'moment';
import React from 'react';
import { DragDropContext, Draggable, Droppable } from 'react-beautiful-dnd';
import { FaGripLines, FaTrash } from 'react-icons/fa';
import { useSearchParams } from 'react-router-dom';
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
  IAVOverviewTemplateSectionItemModel,
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

interface Suggestion {
  index: number;
  text: string;
}

/** OverviewGrid contains the table of items displayed for each overview section. */
export const OverviewGrid: React.FC<IOverviewGridProps> = ({ editable = true, index }) => {
  const { values, setFieldValue } = useFormikContext<IAVOverviewInstanceModel>();
  const [content, { findContent }] = useContent();

  const [showAutoCompleteForIndex, setShowAutoCompleteForIndex] = React.useState<null | number>(
    null,
  );
  const [search, setSearch] = React.useState<{ [key: string]: any }>({
    text: '',
    suggestions: [],
  });
  const [clips, setClips] = React.useState<IOptionItem[]>();
  const [summaries, setSummaries] = React.useState<any[]>();

  const eveningOverviewItemTypeOptions = castEnumToOptions(AVOverviewItemTypeName);
  const items = values.sections[index].items;
  const [params] = useSearchParams();
  const queryDate = params.get('date') ? moment(params.get('date')) : moment(Date.now());
  const startTime = values.sections[index]?.startTime?.split(':');

  /** flag to keep track of when new complete start time is entered and trigger another search
   * for relevant clips
   */
  const shouldFetch = React.useMemo(() => {
    return (
      !!values?.sections[index]?.startTime && !values?.sections[index]?.startTime?.includes('_')
    );
  }, [index, values?.sections]);

  /** fetch pieces of content that are related to the series to display as options for associated clips, search for clips published after the start time if it is specified - otherwise filter based on that day.*/
  React.useEffect(() => {
    if (shouldFetch) {
      findContent({
        seriesId: values.sections[index].seriesId,
        publishedStartOn: !!values.sections[index].startTime
          ? moment(queryDate)
              .set({
                hour: Number(startTime[0]),
                minute: Number(startTime[1]),
                second: Number(startTime[2]),
                millisecond: 0,
              })
              .toISOString()
          : moment(queryDate).startOf('day').toISOString(),
        contentTypes: [],
        sort: ['publishedOn asc'],
      }).then((data) => {
        setClips(data.items.map((c) => new OptionItem(c.headline, c.id)) as IOptionItem[]);
      });
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

    const droppedItemValues = items[droppedItem.source.index];

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

  const handleSelectionChanged = (itemIndex: number, newValue: IOptionItem | undefined) => {
    if (newValue === undefined) return;
    const summary: string = getIn(values, `sections.${index}.items.${itemIndex}.summary`);
    const regex = /(?:timestamp|time stamp)(?:\s+is)?\s+(\d{1,2}:\d{2})/g;
    const match = regex.exec(summary);
    if (match) {
      setFieldValue(
        `sections.${index}.items.${itemIndex}.time`,
        match[1].length === 5 ? `${match[1]}:00` : `0${match[1]}:00`,
      );
    } else {
      const selectedClip = content.searchResults?.items?.find((s) => s.id === newValue?.value);
      const publishedOn = moment(selectedClip?.publishedOn).format('HH:mm:ss');
      setFieldValue(`sections.${index}.items.${itemIndex}.time`, publishedOn);
    }
  };

  const generateListOfSummaries = () => {
    const sections = values?.sections;
    let summaries: any[] = [];
    if (sections && sections.length) {
      sections.forEach((section) => {
        summaries.push(
          ...section?.items?.reduce(function (
            acc: Array<{ index: number; text: string }>,
            current: IAVOverviewSectionItemModel,
            index: number,
          ) {
            if (
              !acc.some((summary) => summary.text === current.summary) // do not display duplicates
            )
              acc.push({ index, text: current.summary });
            return acc;
          },
          []),
        );
      });
    }
    return summaries;
  };

  React.useEffect(() => {
    const summaryList = generateListOfSummaries();
    setSummaries(summaryList);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [values?.sections]);

  const special = [
    'Zeroth',
    'First',
    'Second',
    'Third',
    'Fourth',
    'Fifth',
    'Sixth',
    'Seventh',
    'Eighth',
    'Ninth',
    'Tenth',
    'Eleventh',
    'Twelfth',
    'Thirteenth',
    'Fourteenth',
    'Fifteenth',
    'Sixteenth',
    'Seventeenth',
    'Eighteenth',
    'Nineteenth',
  ];
  const deca = ['Twent', 'Thirt', 'Fort', 'Fift', 'Sixt', 'Sevent', 'Eight', 'Ninet'];

  const stringifyNumber = (n: number) => {
    if (n < 20) return special[n];
    if (n % 10 === 0) return deca[Math.floor(n / 10) - 2] + 'ieth';
    return deca[Math.floor(n / 10) - 2] + 'y-' + special[n % 10];
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
                    {items.map((item, itemIndex) => {
                      const { suggestions } = search;
                      return (
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
                                  onChange={(newValue: unknown) => {
                                    const option = newValue as IOptionItem;
                                    if (option?.value === 'Ad') {
                                      const numberOfExistingAds = items.reduce(
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
                                        `${stringifyNumber(numberOfExistingAds)} commercial break`,
                                      );
                                    }
                                  }}
                                />
                                <FormikTimeInput
                                  key={itemIndex}
                                  name={`sections.${index}.items.${itemIndex}.time`}
                                  width={FieldSize.Small}
                                  placeholder="hh:mm:ss"
                                  disabled={!editable}
                                />
                                <Col
                                  flex="1"
                                  style={{
                                    position: 'relative',
                                    width: '320px',
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
                                  <div>
                                    <FormikTextArea
                                      key={itemIndex + `textarea`}
                                      name={`sections.${index}.items.${itemIndex}.summary`}
                                      rows={item.itemType === AVOverviewItemTypeName.Intro ? 3 : 1}
                                      disabled={!editable || item.itemType === 'Ad'}
                                      maxLength={
                                        item.itemType === AVOverviewItemTypeName.Intro ? 2000 : 90
                                      }
                                      onChange={(e) => {
                                        const value = e.target.value;
                                        setFieldValue(
                                          `sections.${index}.items.${itemIndex}.summary`,
                                          value,
                                        );

                                        // from the potential summaries, generate suggestions that match current input
                                        let suggestions: Suggestion[] = [];
                                        if (value.length > 0 && summaries?.length) {
                                          const regex = new RegExp(`^${value}`, 'i');
                                          suggestions = summaries
                                            .sort()
                                            .filter(
                                              (v: Suggestion) =>
                                                regex.test(v.text) && v.index !== itemIndex,
                                            );
                                        }
                                        setShowAutoCompleteForIndex(itemIndex);
                                        setSearch({ suggestions, text: value });
                                      }}
                                    />
                                  </div>
                                  {suggestions.length > 0 &&
                                    showAutoCompleteForIndex === itemIndex && (
                                      <styled.AutoCompleteContainer>
                                        {suggestions.map((suggestion: Suggestion) => {
                                          return (
                                            <styled.AutoCompleteItem key={suggestion.index}>
                                              <styled.AutoCompleteItemButton
                                                key={suggestion.index}
                                                onClick={() => {
                                                  setFieldValue(
                                                    `sections.${index}.items.${itemIndex}.summary`,
                                                    suggestion.text,
                                                  );
                                                  setShowAutoCompleteForIndex(null);
                                                }}
                                              >
                                                {suggestion.text}
                                              </styled.AutoCompleteItemButton>
                                            </styled.AutoCompleteItem>
                                          );
                                        })}
                                      </styled.AutoCompleteContainer>
                                    )}
                                </Col>
                                <FormikSelect
                                  name={`sections.${index}.items.${itemIndex}.contentId`}
                                  value={clips?.find((c) => c.value === item.contentId)}
                                  options={clips ?? []}
                                  width={FieldSize.Medium}
                                  isDisabled={!editable}
                                  onChange={(newValue) =>
                                    handleSelectionChanged(itemIndex, newValue as IOptionItem)
                                  }
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
