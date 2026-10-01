import React from 'react';
import { Text } from 'tno-core';

import { type ITopicScoreSourceModel } from './interfaces';

export interface ISourcesPaneProps {
  /** Sources that use topics. */
  sources: ITopicScoreSourceModel[];
  /** The selected source. */
  selectedId?: number;
  /** Select a source. */
  onSelect: (source: ITopicScoreSourceModel) => void;
  /** Save a source's default score. */
  onDefaultScoreChange: (source: ITopicScoreSourceModel) => Promise<void>;
}

/**
 * Lists the sources that use topics, with their rule count and inline-editable default score.
 * @param param0 Component properties.
 * @returns Component.
 */
export const SourcesPane: React.FC<ISourcesPaneProps> = ({
  sources,
  selectedId,
  onSelect,
  onDefaultScoreChange,
}) => {
  const [filter, setFilter] = React.useState('');
  const [drafts, setDrafts] = React.useState<Record<number, string>>({});

  const value = filter.trim().toLowerCase();
  const items = value
    ? sources.filter(
        (s) => s.name.toLowerCase().includes(value) || s.code.toLowerCase().includes(value),
      )
    : sources;

  const commitDefaultScore = async (source: ITopicScoreSourceModel) => {
    const draft = drafts[source.id];
    if (draft === undefined) return;
    const trimmed = draft.trim();
    const score = trimmed === '' ? undefined : Number(trimmed);
    if (score !== undefined && (!Number.isInteger(score) || score < 0)) return;
    if (score !== source.topicDefaultScore) {
      await onDefaultScoreChange({ ...source, topicDefaultScore: score });
    }
    setDrafts(({ [source.id]: _, ...rest }) => rest);
  };

  return (
    <div className="sources-pane">
      <Text
        name="sourceFilter"
        placeholder="Filter by name or code"
        value={filter}
        onChange={(e) => setFilter(e.target.value)}
      />
      <div className="sources-header">
        <span>Source</span>
        <span title="Rules">Rules</span>
        <span title="Score when no rule matches">Default</span>
      </div>
      <div className="sources-list">
        {items.map((source) => {
          const draft = drafts[source.id];
          const draftValue = draft ?? `${source.topicDefaultScore ?? ''}`;
          const isInvalid =
            draft !== undefined &&
            draft.trim() !== '' &&
            (!Number.isInteger(Number(draft)) || Number(draft) < 0);
          return (
            <div
              key={source.id}
              className={`source-row${source.id === selectedId ? ' selected' : ''}`}
              onClick={() => onSelect(source)}
            >
              <span className="source-name" title={`${source.code}: ${source.name}`}>
                {source.name}
                {!source.useInTopics && source.seriesUseInTopics && (
                  <span className="hint" title="Only some of this source's series use topics">
                    {' '}
                    (series)
                  </span>
                )}
              </span>
              <span className="rule-count">{source.ruleCount}</span>
              <div className="default-score" onClick={(e) => e.stopPropagation()}>
                <Text
                  name={`defaultScore-${source.id}`}
                  aria-label={`Default score for ${source.name}`}
                  title="Blank scores unmatched content 0"
                  error={isInvalid ? 'A whole number of 0 or more' : undefined}
                  value={draftValue}
                  placeholder="0"
                  inputMode="numeric"
                  width="5ch"
                  onChange={(e) => setDrafts((d) => ({ ...d, [source.id]: e.target.value }))}
                  onBlur={() => commitDefaultScore(source)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') (e.target as HTMLInputElement).blur();
                    if (e.key === 'Escape') setDrafts(({ [source.id]: _, ...rest }) => rest);
                  }}
                />
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};
