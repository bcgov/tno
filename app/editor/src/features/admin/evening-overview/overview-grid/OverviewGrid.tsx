import { set } from 'lodash';
import { DragDropContext, Draggable, Droppable } from 'react-beautiful-dnd';
import { FaTrash } from 'react-icons/fa';
import { useEveningOverviews } from 'store/hooks/admin/useEveningOverviews';
import { Col, FieldSize, OptionItem, Row, Select, Show, TextArea, TimeInput } from 'tno-core';

import { EveningOverviewItemType, IEveningOverviewItem } from '../interfaces';
import * as styled from './styled';

export interface IOverviewGridProps {
  items: IEveningOverviewItem[];
  setItems: (items: IEveningOverviewItem[]) => void;
}

/** OverviewGrid contains the table of items displayed for each overview section. */
export const OverviewGrid: React.FC<IOverviewGridProps> = ({ items, setItems }) => {
  const options: OptionItem[] = [];
  const [, api] = useEveningOverviews();
  Object.keys(EveningOverviewItemType).forEach(function (key, index) {
    options.push(
      new OptionItem(EveningOverviewItemType[key].name, EveningOverviewItemType[key].id),
    );
  });
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
    setItems(updatedList);
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
            <Col className="summary-header">Summary</Col>
          </Row>
          <div className="contents">
            <DragDropContext onDragEnd={handleDrop}>
              <Droppable droppableId="list-container">
                {(provided) => (
                  <div
                    className="list-container"
                    {...provided.droppableProps}
                    ref={provided.innerRef}
                  >
                    {items.map((item, index) => (
                      <Draggable key={index} draggableId={index.toString()} index={index}>
                        {(provided) => (
                          <div
                            className="item-container"
                            ref={provided.innerRef}
                            {...provided.dragHandleProps}
                            {...provided.draggableProps}
                          >
                            <Row className="rows">
                              <Col>
                                <Select
                                  width={FieldSize.Small}
                                  options={options}
                                  name="item-type"
                                  value={
                                    typeof item.itemType === 'number'
                                      ? options.find((o) => o.value === item.itemType)
                                      : options.find((o) => o.label === item.itemType)
                                  }
                                  onChange={(newValue: any) => {
                                    setItems(
                                      set([...items], index, { ...item, itemType: newValue.value }),
                                    );
                                  }}
                                />
                              </Col>
                              <Col>
                                <TimeInput
                                  value={item.time}
                                  width={FieldSize.Small}
                                  onChange={(e) =>
                                    setItems(
                                      set([...items], index, { ...item, time: e.target.value }),
                                    )
                                  }
                                />
                              </Col>
                              <Col>
                                <Row>
                                  <TextArea
                                    name="summary"
                                    value={item.summary}
                                    width={FieldSize.Large}
                                    rows={1}
                                    className="summary"
                                    onChange={(e) => {
                                      setItems(
                                        set([...items], index, {
                                          ...item,
                                          summary: e.target.value,
                                        }),
                                      );
                                    }}
                                  />
                                  <FaTrash
                                    className="clear-item"
                                    onClick={async () => {
                                      await api.deleteOverviewSectionItem(item);
                                      setItems(items.filter((x) => x !== item));
                                    }}
                                  />
                                </Row>
                              </Col>
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
