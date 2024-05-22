import { Topic } from 'features/content';
import React from 'react';
import { FaRegClipboard } from 'react-icons/fa';
import { Link } from 'react-router-dom';
import {
  CellDate,
  CellEllipsis,
  IContentTopicModel,
  IFolderContentModel,
  ITableHookColumn,
  ITableInternalCell,
  ITopicModel,
  OptionItem,
  Select,
  Show,
  Spinner,
} from 'tno-core';

// item with id of 1 is the magic [Not Applicable] topic
const topicIdNotApplicable = 1;

const maxTopicScore: number = 200;
// create an array with values 0-maxScore
const possibleScores = Array.from(Array(maxTopicScore + 1).keys()).map(
  (item) => new OptionItem('' + item, item),
);

export const useColumns = (
  topics: ITopicModel[],
  handleSubmit: (values: IFolderContentModel) => Promise<void>,
): ITableHookColumn<IFolderContentModel>[] => {
  const [isContentUpdating, setIsContentUpdating] = React.useState<number[]>([]);
  const toggleContentUpdatingStatus = (contentId: number) => {
    setIsContentUpdating((state) => {
      let returnVal: number[];
      if (state.findIndex((el: number) => el === contentId) > -1) {
        returnVal = state.filter((el) => el !== contentId);
      } else {
        returnVal = [...state, contentId];
      }
      return returnVal;
    });
  };
  const isRowContentUpdating = (contentId: number) => {
    return isContentUpdating.findIndex((el: number) => el === contentId) > -1;
  };

  const handleTopicChange = async (
    topic: ITopicModel,
    cell: ITableInternalCell<IFolderContentModel>,
  ) => {
    if (
      cell.original.content &&
      ((topic && cell.original.content.topics.length === 0) ||
        (!topic && cell.original.content.topics.length > 0) ||
        topic?.id !== cell.original.content?.topics[0].id)
    ) {
      const updatedFolderContent = {
        ...cell.original,
      } as IFolderContentModel;
      updatedFolderContent.content!.topics = [
        {
          ...(topic as IContentTopicModel),
          // if the original topic was "Not Applicable", it may be because no topic was set
          // this logic below avoids a reference to an empty array
          score:
            topic!.id === topicIdNotApplicable
              ? 0
              : cell.original.content!.topics.length === 0
              ? 0
              : cell.original.content!.topics[0].score,
        },
      ];
      toggleContentUpdatingStatus(cell.original.contentId);
      await handleSubmit(updatedFolderContent).then(() => {
        toggleContentUpdatingStatus(cell.original.contentId);
      });
    }
  };

  const handleScoreChange = async (
    newValue: any,
    cell: ITableInternalCell<IFolderContentModel>,
  ) => {
    if (cell.original.content) {
      let newScore = (newValue as OptionItem).value;
      const updatedFolderContent = {
        ...cell.original,
      } as IFolderContentModel;
      updatedFolderContent.content!.topics[0].score = newScore ? +newScore : 0;
      toggleContentUpdatingStatus(cell.original.contentId);
      await handleSubmit(updatedFolderContent).then(() => {
        toggleContentUpdatingStatus(cell.original.contentId);
      });
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

  const result: ITableHookColumn<IFolderContentModel>[] = [
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
        var cellTextComponents = [];
        if (cell.original.content!.page && cell.original.content!.page.length > 0)
          cellTextComponents.push(cell.original.content!.page);
        if (cell.original.content!.section && cell.original.content!.section.length > 0)
          cellTextComponents.push(cell.original.content!.section);
        var cellText: string =
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
            value={
              !!cell.original.content!.topics!.length
                ? cell.original.content!.topics[0].id
                : topicIdNotApplicable
            }
            handleTopicChange={async (e: any) => await handleTopicChange(e, cell)}
          />
        );
      },
    },
    {
      label: 'Score',
      accessor: 'name',
      width: 0.6,
      cell: (cell) => {
        return (
          <>
            <Select
              name="score"
              isDisabled={
                isRowContentUpdating(cell.original.contentId) ||
                (cell.original.content!.topics!.length > 0 &&
                  cell.original.content!.topics![0].id === topicIdNotApplicable)
              }
              isClearable={false}
              clearValue={''}
              className={
                'score-select ' +
                (isRowContentUpdating(cell.original.contentId) ? 'lock-control' : '')
              }
              options={possibleScores.filter(
                // remove this filter if the editor needs to be able to override to any value they want
                (s) => s.value <= (cell.original.maxTopicScore ?? maxTopicScore),
              )}
              value={possibleScores?.find(
                (o) =>
                  o.value ===
                  (cell.original.content!.topics!.length > 0
                    ? cell.original.content!.topics![0].score
                    : 0),
              )}
              onChange={(newValue) => {
                handleScoreChange(newValue, cell);
              }}
            />
            <div className="maxScore">
              &nbsp;&le;&nbsp;
              <Show visible={cell.original!.maxTopicScore !== undefined}>
                <dfn
                  title="max score as calculated by matched rule"
                  className="score-max-hint-text"
                >
                  {cell.original!.maxTopicScore}
                </dfn>
              </Show>
              <Show visible={cell.original!.maxTopicScore === undefined}>
                <dfn title="no rule match" className="score-max-no-rule-match">
                  {maxTopicScore}
                </dfn>
              </Show>
            </div>
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
