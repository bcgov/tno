import { StrictModeDroppable } from 'features/admin/automation/StrictModeDroppable';
import React from 'react';
import { DragDropContext, Draggable, type DropResult } from 'react-beautiful-dnd';
import { FaGripVertical } from 'react-icons/fa';
import { useLookup } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  IconButton,
  type ITopicScoreRuleModel,
  OptionItem,
  Row,
  Select,
  Show,
  Text,
} from 'tno-core';

import { type ITopicScoreSourceModel } from './interfaces';
import { formatPageRange, formatRange } from './utils';

export interface IRulesPaneProps {
  /** The selected source. */
  source: ITopicScoreSourceModel;
  /** The source's rules in evaluation order. */
  rules: ITopicScoreRuleModel[];
  /** Add a rule. */
  onAdd: () => void;
  /** Edit a rule. */
  onEdit: (rule: ITopicScoreRuleModel) => void;
  /** Save a new order. */
  onReorder: (rules: ITopicScoreRuleModel[]) => Promise<void>;
}

const formatImage = (hasImage?: boolean) =>
  hasImage === undefined || hasImage === null ? '' : hasImage ? 'Yes' : 'No';

/**
 * The ordered rules of one source. The first rule that matches a story sets its score, so the
 * order matters; rows are dragged to reorder, which saves the new order for this source only.
 * @param param0 Component properties.
 * @returns Component.
 */
export const RulesPane: React.FC<IRulesPaneProps> = ({
  source,
  rules,
  onAdd,
  onEdit,
  onReorder,
}) => {
  const [{ series }] = useLookup();
  const [seriesFilter, setSeriesFilter] = React.useState<number | ''>('');
  const [sectionFilter, setSectionFilter] = React.useState('');

  const seriesName = (id?: number) => series.find((s) => s.id === id)?.name ?? '';
  const seriesOptions = [
    new OptionItem('All series', ''),
    ...Array.from(new Set(rules.filter((r) => r.seriesId).map((r) => r.seriesId!))).map(
      (id) => new OptionItem(seriesName(id) || `${id}`, id),
    ),
  ];

  const section = sectionFilter.trim().toLowerCase();
  const isFiltered = seriesFilter !== '' || section !== '';
  const items = rules.filter(
    (r) =>
      (seriesFilter === '' || r.seriesId === seriesFilter) &&
      (section === '' || (r.section ?? '').toLowerCase().includes(section)),
  );

  const handleDragEnd = async (result: DropResult) => {
    if (!result.destination || result.destination.index === result.source.index) return;
    const reordered = [...rules];
    const [moved] = reordered.splice(result.source.index, 1);
    reordered.splice(result.destination.index, 0, moved);
    await onReorder(reordered);
  };

  return (
    <div className="rules-pane">
      <Row className="rules-toolbar" gap="0.5rem" alignItems="flex-end">
        <h2>{source.name}</h2>
        <Select
          name="seriesFilter"
          label="Series"
          width="20ch"
          options={seriesOptions}
          value={seriesOptions.find((o) => o.value === seriesFilter)}
          onChange={(o) => setSeriesFilter(((o as OptionItem)?.value as number) ?? '')}
        />
        <Text
          name="sectionFilter"
          label="Section"
          width="20ch"
          value={sectionFilter}
          onChange={(e) => setSectionFilter(e.target.value)}
        />
        <IconButton iconType="plus" label="Add rule" onClick={onAdd} />
      </Row>
      <p className="hint">
        Rules are checked top to bottom and the first match sets the score.
        {source.topicDefaultScore !== undefined && source.topicDefaultScore !== null
          ? ` When none match, the source default of ${source.topicDefaultScore} applies.`
          : ' When none match, the score is 0.'}
        <Show visible={isFiltered}> Clear the filters to reorder.</Show>
      </p>
      <div className="rules-grid">
        <div className="rules-grid-header">
          <span></span>
          <span>#</span>
          <span>Series</span>
          <span>Section</span>
          <span>Page</span>
          <span>Image</span>
          <span>Time</span>
          <span>Characters</span>
          <span>Score</span>
          <span></span>
        </div>
        <DragDropContext onDragEnd={handleDragEnd}>
          <StrictModeDroppable droppableId="rules">
            {(provided) => (
              <div ref={provided.innerRef} {...provided.droppableProps}>
                {items.map((rule, index) => (
                  <Draggable
                    key={rule.id}
                    draggableId={`${rule.id}`}
                    index={index}
                    isDragDisabled={isFiltered}
                  >
                    {(drag) => (
                      <div
                        ref={drag.innerRef}
                        {...drag.draggableProps}
                        className="rules-grid-row"
                        onDoubleClick={() => onEdit(rule)}
                      >
                        <span
                          {...drag.dragHandleProps}
                          className={`drag-handle${isFiltered ? ' disabled' : ''}`}
                          title={isFiltered ? 'Clear the filters to reorder' : 'Drag to reorder'}
                        >
                          <FaGripVertical />
                        </span>
                        <span>{rules.indexOf(rule) + 1}</span>
                        <span>{seriesName(rule.seriesId)}</span>
                        <span>{rule.section}</span>
                        <span>{formatPageRange(rule.pageMin, rule.pageMax)}</span>
                        <span>{formatImage(rule.hasImage)}</span>
                        <span>{formatRange(rule.timeMin, rule.timeMax, true)}</span>
                        <span>{formatRange(rule.characterMin, rule.characterMax)}</span>
                        <span className="score">{rule.score}</span>
                        <span>
                          <Button variant={ButtonVariant.link} onClick={() => onEdit(rule)}>
                            Edit
                          </Button>
                        </span>
                      </div>
                    )}
                  </Draggable>
                ))}
                {provided.placeholder}
              </div>
            )}
          </StrictModeDroppable>
        </DragDropContext>
        <Show visible={!items.length}>
          <div className="rules-grid-empty">
            {rules.length ? 'No rules match the filters.' : 'This source has no rules.'}
          </div>
        </Show>
      </div>
    </div>
  );
};
