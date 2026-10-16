import { StrictModeDroppable } from 'features/admin/automation/StrictModeDroppable';
import React from 'react';
import { DragDropContext, Draggable, type DropResult } from 'react-beautiful-dnd';
import { FaArrowDown, FaArrowUp, FaGripVertical, FaTrash } from 'react-icons/fa';
import { type createFilter } from 'react-select';
import {
  Button,
  ButtonVariant,
  FieldSize,
  filterEnabledOptions,
  FormikSelect,
  FormikText,
  type IOptionItem,
  Row,
} from 'tno-core';

import { type ISyndicationFeed } from '../interfaces';
import * as styled from './styled';

export interface IFeedListProps {
  /** The feeds in priority order, bound to 'configuration.feeds'. */
  feeds: ISyndicationFeed[];
  /** Source options, the value is the source code. */
  sources: IOptionItem[];
  /** Matches a source option to the search text. */
  sourceFilter: ReturnType<typeof createFilter<unknown>>;
  /** Called with the feeds after they are reordered or removed. */
  onChange: (feeds: ISyndicationFeed[]) => void;
}

/**
 * The syndication feeds in priority order. Drag a row by its handle, or use the arrows, to change
 * its priority.
 */
export const FeedList = React.forwardRef<HTMLDivElement, IFeedListProps>(
  ({ feeds, sources, sourceFilter, onChange }, ref) => {
    // Feeds have no id, so each row keeps a key that moves with it.
    const keys = React.useRef<string[]>([]);
    const nextKey = React.useRef(0);
    if (keys.current.length < feeds.length) {
      keys.current = [
        ...keys.current,
        ...feeds.slice(keys.current.length).map(() => `feed-${nextKey.current++}`),
      ];
    } else if (keys.current.length > feeds.length) {
      keys.current = keys.current.slice(0, feeds.length);
    }

    const move = (from: number, to: number) => {
      if (from === to || to < 0 || to >= feeds.length) return;
      const reorder = <T,>(items: T[]) => {
        const updated = [...items];
        const [item] = updated.splice(from, 1);
        updated.splice(to, 0, item);
        return updated;
      };
      keys.current = reorder(keys.current);
      onChange(reorder(feeds));
    };

    const remove = (index: number) => {
      keys.current = keys.current.filter((_, i) => i !== index);
      onChange(feeds.filter((_, i) => i !== index));
    };

    const handleDragEnd = (result: DropResult) => {
      if (result.destination) move(result.source.index, result.destination.index);
    };

    return (
      <styled.FeedList ref={ref}>
        <div className="feed-header">
          <span></span>
          <span>#</span>
          <span>Feed URL</span>
          <span>Source</span>
          <span></span>
        </div>
        <DragDropContext onDragEnd={handleDragEnd}>
          <StrictModeDroppable droppableId="feeds">
            {(provided) => (
              <div className="feed-rows" ref={provided.innerRef} {...provided.droppableProps}>
                {feeds.map((feed, index) => (
                  <Draggable
                    key={keys.current[index]}
                    draggableId={keys.current[index]}
                    index={index}
                  >
                    {(drag, snapshot) => (
                      <div
                        ref={drag.innerRef}
                        {...drag.draggableProps}
                        className={`feed-row${snapshot.isDragging ? ' dragging' : ''}`}
                      >
                        <span
                          {...drag.dragHandleProps}
                          className="drag-handle"
                          title="Drag to change the priority"
                          aria-label={`Drag feed ${index + 1} to change its priority`}
                        >
                          <FaGripVertical />
                        </span>
                        <span>{index + 1}</span>
                        <FormikText
                          name={`configuration.feeds.${index}.url`}
                          placeholder="https://"
                          width={FieldSize.Stretch}
                          required
                        />
                        <FormikSelect
                          width={FieldSize.Stretch}
                          name={`configuration.feeds.${index}.source`}
                          options={filterEnabledOptions(sources, feed.source)}
                          value={sources.find((option) => option.value === feed.source) ?? ''}
                          placeholder="Ingest source"
                          filterOption={sourceFilter}
                          isClearable
                        />
                        <Row gap="0.25rem" nowrap>
                          <Button
                            variant={ButtonVariant.secondary}
                            tooltip="Higher priority"
                            disabled={index === 0}
                            onClick={() => move(index, index - 1)}
                          >
                            <FaArrowUp />
                          </Button>
                          <Button
                            variant={ButtonVariant.secondary}
                            tooltip="Lower priority"
                            disabled={index >= feeds.length - 1}
                            onClick={() => move(index, index + 1)}
                          >
                            <FaArrowDown />
                          </Button>
                          <Button
                            variant={ButtonVariant.danger}
                            tooltip="Remove feed"
                            onClick={() => remove(index)}
                          >
                            <FaTrash />
                          </Button>
                        </Row>
                      </div>
                    )}
                  </Draggable>
                ))}
                {provided.placeholder}
              </div>
            )}
          </StrictModeDroppable>
        </DragDropContext>
      </styled.FeedList>
    );
  },
);
