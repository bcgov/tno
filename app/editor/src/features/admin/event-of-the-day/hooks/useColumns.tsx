import { Topic } from 'features/content';
import React from 'react';
import { FaRegClipboard } from 'react-icons/fa';
import { Link } from 'react-router-dom';
import {
  Button,
  ButtonVariant,
  CellDate,
  CellEllipsis,
  type IContentTopicModel,
  type IFolderContentModel,
  type ITableHookColumn,
  type ITableInternalCell,
  type ITopicModel,
  OptionItem,
  Select,
  Show,
  Spinner,
} from 'tno-core';

const maxTopicScore: number = 200;
// create an array with values 0-maxScore
const possibleScores = Array.from(Array(maxTopicScore + 1).keys()).map(
  (item) => new OptionItem('' + item, item),
);

export const useColumns = (
  topics: ITopicModel[],
  handleSubmit: (values: IFolderContentModel) => Promise<void>,
  handleReset: (values: IFolderContentModel, topic: IContentTopicModel) => Promise<void>,
): Array<ITableHookColumn<IFolderContentModel>> => {
  const [isContentUpdating, setIsContentUpdating] = React.useState<number[]>([]);

  // The system [Not Applicable] topic is identified by its flag, not by its id or name.
  const topicIdNotApplicable = topics.find((t) => t.isSystem)?.id;

  const toggleContentUpdatingStatus = (contentId: number) => {
    setIsContentUpdating((state) =>
      state.includes(contentId) ? state.filter((el) => el !== contentId) : [...state, contentId],
    );
  };
  const isRowContentUpdating = (contentId: number) => isContentUpdating.includes(contentId);

  /** Submit a copy of the row with the specified topics, never mutating the table data. */
  const submitTopics = async (
    cell: ITableInternalCell<IFolderContentModel>,
    topics: IContentTopicModel[],
  ) => {
    if (!cell.original.content) return;
    const updatedFolderContent: IFolderContentModel = {
      ...cell.original,
      content: { ...cell.original.content, topics },
    };
    toggleContentUpdatingStatus(cell.original.contentId);
    try {
      await handleSubmit(updatedFolderContent);
    } finally {
      toggleContentUpdatingStatus(cell.original.contentId);
    }
  };

  const handleTopicChange = async (
    topic: ITopicModel | undefined,
    cell: ITableInternalCell<IFolderContentModel>,
  ) => {
    const current = cell.original.content?.topics[0];
    if (!cell.original.content || !topic || topic.id === current?.id) return;

    // An editor's chosen score follows the story to its new topic; otherwise the new topic takes
    // the calculated score, which the API stores as calculated rather than as an override.
    const score = current?.isScoreOverridden
      ? current.score
      : cell.original.calculatedTopicScore ?? current?.score ?? 0;
    await submitTopics(cell, [{ ...(topic as IContentTopicModel), score }]);
  };

  const handleScoreChange = async (
    newValue: any,
    cell: ITableInternalCell<IFolderContentModel>,
  ) => {
    const current = cell.original.content?.topics[0];
    if (!current) return;
    const newScore = (newValue as OptionItem).value;
    await submitTopics(cell, [{ ...current, score: newScore ? +newScore : 0 }]);
  };

  const handleResetScore = async (cell: ITableInternalCell<IFolderContentModel>) => {
    const current = cell.original.content?.topics[0];
    if (!current) return;
    toggleContentUpdatingStatus(cell.original.contentId);
    try {
      await handleReset(cell.original, current);
    } finally {
      toggleContentUpdatingStatus(cell.original.contentId);
    }
  };

  const handleCopyTitle = (event: any, cell: ITableInternalCell<IFolderContentModel>) => {
    navigator.clipboard.writeText(cell.original.content!.headline);
    // animate the clipboard icon to show something happened
    event.target.classList.toggle('animate');
    setTimeout(() => {
      event.target.classList.toggle('animate');
    }, 500);
  };

  const result: Array<ITableHookColumn<IFolderContentModel>> = [
    {
      label: 'Topic Name',
      accessor: 'name',
      width: 1,
      cell: (cell) => {
        return (
          <>
            <Link
              to={`/contents/${cell.original.contentId}`}
              target="blank"
              className={isRowContentUpdating(cell.original.contentId) ? 'lock-control' : ''}
            >
              {cell.original.content?.headline}
            </Link>
            <FaRegClipboard
              className="clipboard-icon"
              title="Copy Title to clipboard"
              onClick={(e) => {
                handleCopyTitle(e, cell);
              }}
            />
          </>
        );
      },
    },
    {
      accessor: 'pageSection',
      label: 'Page:Section',
      cell: (cell) => {
        const cellTextComponents = [];
        if (cell.original.content!.page && cell.original.content!.page.length > 0) {
          cellTextComponents.push(cell.original.content!.page);
        }
        if (cell.original.content!.section && cell.original.content!.section.length > 0) {
          cellTextComponents.push(cell.original.content!.section);
        }
        const cellText: string =
          cellTextComponents.length === 2
            ? cellTextComponents.join(':')
            : cellTextComponents.join('');
        return <CellEllipsis>{cellText}</CellEllipsis>;
      },
      width: '20ch',
      hAlign: 'left',
    },
    {
      accessor: 'publishedOn',
      label: 'Pub Date/Time',
      cell: (cell) => <CellDate value={cell.original.content!.publishedOn} />,
      width: '20ch',
      hAlign: 'center',
    },
    {
      label: 'Topic',
      accessor: 'topic',
      cell: (cell) => {
        return (
          <Topic
            name={'topic'}
            isDisabled={isRowContentUpdating(cell.original.contentId)}
            className={
              'topic-select ' +
              (isRowContentUpdating(cell.original.contentId) ? 'lock-control' : '')
            }
            filteredTopics={topics}
            value={cell.original.content!.topics[0]?.id ?? topicIdNotApplicable}
            handleTopicChange={async (topic) => {
              await handleTopicChange(topic, cell);
            }}
          />
        );
      },
    },
    {
      label: 'Score',
      accessor: 'name',
      width: 0.8,
      cell: (cell) => {
        const topic = cell.original.content!.topics[0];
        const calculated = cell.original.calculatedTopicScore;
        return (
          <>
            <Select
              name="score"
              isDisabled={
                isRowContentUpdating(cell.original.contentId) ||
                !topic ||
                topic.id === topicIdNotApplicable
              }
              isClearable={false}
              clearValue={''}
              className={
                'score-select ' +
                (isRowContentUpdating(cell.original.contentId) ? 'lock-control' : '')
              }
              options={possibleScores.filter(
                // An editor can choose any score up to the calculated one.
                (s) => s.value <= (calculated ?? maxTopicScore),
              )}
              value={possibleScores.find((o) => o.value === (topic?.score ?? 0))}
              onChange={(newValue) => {
                handleScoreChange(newValue, cell);
              }}
            />
            <div className="maxScore">
              &nbsp;&le;&nbsp;
              <dfn
                title={
                  calculated !== undefined
                    ? 'score calculated by the topic score rules'
                    : 'not scored by the topic score rules'
                }
                className={
                  calculated !== undefined ? 'score-max-hint-text' : 'score-max-no-rule-match'
                }
              >
                {calculated ?? maxTopicScore}
              </dfn>
            </div>
            <Show visible={!!topic?.isScoreOverridden}>
              <Button
                variant={ButtonVariant.link}
                className="reset-score"
                title="Clear the override and use the calculated score"
                disabled={isRowContentUpdating(cell.original.contentId)}
                onClick={() => handleResetScore(cell)}
              >
                Reset
              </Button>
            </Show>
          </>
        );
      },
    },
    {
      label: '',
      accessor: 'busy',
      width: '4ch',
      cell: (cell) => (
        <>
          <Show visible={isRowContentUpdating(cell.original.contentId)}>
            <Spinner size="1rem" />
          </Show>
        </>
      ),
    },
  ];

  return result;
};
