import { calcTopicScore } from 'features/content/form/utils';
import React from 'react';
import { Link } from 'react-router-dom';
import { useLookup } from 'store/hooks';
import {
  CellDate,
  FieldSize,
  getSortableOptions,
  IContentTopicModel,
  IFolderContentModel,
  ITableHookColumn,
  OptionItem,
  Select,
  TopicTypeName,
} from 'tno-core';

// create an array with values 0-100 - min score is 0, max is 100
const possibleScores = Array.from(Array(101).keys()).map((item) => new OptionItem('' + item, item));

export const useColumns = (
  handleSubmit: (values: IFolderContentModel) => Promise<void>,
  loading: boolean,
): ITableHookColumn<IFolderContentModel>[] => {
  const [{ topics, rules }] = useLookup();
  const [folderContentModel, setFolderContentModel] = React.useState<IFolderContentModel>();

  const handleTopicChange = async (event: any, cell: any) => {
    const topic = topics.find((x) => x.id === (event as OptionItem)?.value) ?? undefined;
    if (
      (topic && cell.original.content.topics.length === 0) ||
      (!topic && cell.original.content.topics.length > 0) ||
      topic?.id !== cell.original.content.topics[0].id
    ) {
      const updatedFolderContent = {
        ...cell.original,
      } as IFolderContentModel;
      updatedFolderContent.content!.topics = topic
        ? [
            {
              ...(topic as IContentTopicModel),
              score:
                cell.original.content!.topics!.length > 0
                  ? cell.original.content!.topics![0].score
                  : 0,
            },
          ]
        : []; // user chose 'Not Applicable' as Topic
      setFolderContentModel(updatedFolderContent);
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
      setFolderContentModel(updatedFolderContent);
      await handleSubmit(updatedFolderContent);
    }
  };

  const getTopicOptions = (cell: any) => {
    return getSortableOptions(
      topics,
      cell.original.content!.topics?.length ? cell.original.content!.topics[0].id : undefined,
      [new OptionItem('[Not Applicable]', 0)],
      (item) =>
        new OptionItem(
          (
            <div className={item.id > 1 ? `type-${item.topicType}` : 'type-none'}>
              {item.topicType === TopicTypeName.Issues
                ? item.name
                : `${item.name} (${item.topicType})`}
            </div>
          ),
          item.id,
          item.isEnabled,
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
            {cell.original.content?.headline} -{cell.original.content!.topics!.length}
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
                  : 0),
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
              isDisabled={loading || cell.original.content!.topics!.length === 0}
              isClearable={false}
              width="10ch"
              options={possibleScores.filter(
                (s) =>
                  s.value <=
                  calcTopicScore(
                    rules,
                    cell.original.content!.source!.id,
                    cell.original.content!.body!.length,
                    cell.original.content!.publishedOn,
                    cell.original.content!.page,
                    cell.original.content!.section,
                    cell.original.content!.seriesId,
                  ),
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
              &nbsp;&lt;=&nbsp;
              {calcTopicScore(
                rules,
                cell.original.content!.source!.id,
                cell.original.content!.body!.length,
                cell.original.content!.publishedOn,
                cell.original.content!.page,
                cell.original.content!.section,
                cell.original.content!.seriesId,
              )}
            </div>
          </>
        );
      },
    },
  ];

  return result;
};
