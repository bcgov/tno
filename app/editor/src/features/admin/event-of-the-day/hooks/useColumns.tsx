import React from 'react';
import { Link } from 'react-router-dom';
import { useContent, useLookup } from 'store/hooks';
import {
  CellDate,
  FieldSize,
  getSortableOptions,
  IContentTopicModel,
  IFolderContentModel,
  ITableHookColumn,
  OptionItem,
  Select,
  Show,
  TopicTypeName,
} from 'tno-core';

// item with id of 1 is the magic [Not Applicable] topic
const topicIdNotApplicable = 1;

const maxTopicScore: number = 200;
// create an array with values 0-maxScore
const possibleScores = Array.from(Array(maxTopicScore + 1).keys()).map(
  (item) => new OptionItem('' + item, item),
);

export const useColumns = (
  handleSubmit: (values: IFolderContentModel) => Promise<void>,
  loading: boolean,
): ITableHookColumn<IFolderContentModel>[] => {
  const [{ topics }] = useLookup();

  const handleTopicChange = async (event: any, cell: any) => {
    const topic = topics.find((x) => x.id === (event as OptionItem)?.value);
    if (
      (topic && cell.original.content.topics.length === 0) ||
      (!topic && cell.original.content.topics.length > 0) ||
      topic?.id !== cell.original.content.topics[0].id
    ) {
      const updatedFolderContent = {
        ...cell.original,
      } as IFolderContentModel;
      updatedFolderContent.content!.topics = [
        {
          ...(topic as IContentTopicModel),
          score: topic!.id > topicIdNotApplicable ? cell.original.content!.topics![0].score : 0,
        },
      ];
      await handleSubmit(updatedFolderContent);
    }
  };

  const handleScoreChange = async (event: any, cell: any) => {
    let newScore = (event as OptionItem).value;
    if (newScore) {
      const updatedFolderContent = {
        ...cell.original,
      } as IFolderContentModel;
      updatedFolderContent.content!.topics![0].score = +newScore;
      await handleSubmit(updatedFolderContent);
    }
  };

  const getTopicOptions = (cell: any) => {
    return getSortableOptions(
      topics,
      cell.original.content!.topics?.length ? cell.original.content!.topics[0].id : undefined,
      undefined,
      (item) =>
        new OptionItem(
          (
            <div
              className={
                (item.id > 1 ? `type-${item.topicType}` : 'type-none') +
                // This extra style exists only to flag disabled topics that are disabled.
                // These could show up because of migration from TNO, or through changes to
                // content and topics that are possible
                (!item.isEnabled ? ' type-disabled' : '')
              }
            >
              {item.topicType === TopicTypeName.Issues
                ? item.name
                : `${item.name} (${item.topicType})`}
            </div>
          ),
          item.id,
          !item.isEnabled,
        ),
      (a, b) => {
        if (a.topicType < b.topicType) return -1;
        if (a.topicType > b.topicType) return 1;
        if (a.sortOrder < b.sortOrder) return -1;
        if (a.sortOrder > b.sortOrder) return 1;
        if (a.name < b.name) return -1;
        if (a.name > b.name) return 1;
        return 0;
      },
    );
  };

  const result: ITableHookColumn<IFolderContentModel>[] = [
    {
      label: 'Topic Name',
      accessor: 'name',
      width: 1,
      cell: (cell) => {
        return (
          <Link to={`/contents/${cell.original.contentId}`} target="blank">
            {cell.original.content?.headline}
          </Link>
        );
      },
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
      width: 1,
      cell: (cell) => {
        return (
          <Select
            name="topic"
            options={getTopicOptions(cell)}
            isDisabled={loading}
            isClearable={false}
            value={getTopicOptions(cell)?.find(
              (o) =>
                o.value ===
                (cell.original.content!.topics!.length > 0
                  ? cell.original.content!.topics![0].id
                  : topicIdNotApplicable),
            )}
            width={FieldSize.Medium}
            onChange={async (e: any) => await handleTopicChange(e, cell)}
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
                loading ||
                (cell.original.content!.topics!.length > 0 &&
                  cell.original.content!.topics![0].id === topicIdNotApplicable)
              }
              isClearable={false}
              width="10ch"
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
              onChange={async (e: any) => await handleScoreChange(e, cell)}
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
              <Show visible={cell.original!.maxTopicScore == undefined}>
                <dfn title="no rule match" className="score-max-no-rule-match">
                  {maxTopicScore}
                </dfn>
              </Show>
            </div>
          </>
        );
      },
    },
  ];

  return result;
};
