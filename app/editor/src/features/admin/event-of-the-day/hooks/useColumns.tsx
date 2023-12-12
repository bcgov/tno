import { calcTopicScore } from 'features/content/form/utils';
import React from 'react';
import { FaSave } from 'react-icons/fa';
import { Link } from 'react-router-dom';
import { useLookup } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  CellDate,
  Col,
  FieldSize,
  getSortableOptions,
  IContentTopicModel,
  IFolderContentModel,
  ITableHookColumn,
  OptionItem,
  Select,
  Text,
} from 'tno-core';

export const useColumns = (
  handleSubmit: (values: IFolderContentModel) => Promise<void>,
  loading: boolean,
): ITableHookColumn<IFolderContentModel>[] => {
  const [{ topics, rules }] = useLookup();
  const [folderContentModel, setFolderContentModel] = React.useState<IFolderContentModel>();
  // const [ possibleScores, setPossibleScores ] = React.useState<number[]>();

  // create an array with values 0-100 - min score is 0, max is 100
  const possibleScores = Array.from(Array(101).keys()).map(
    (item) => new OptionItem('' + item, item),
  );

  const handleTopicChange = async (event: any, cell: any) => {
    const topic = topics.find((x) => x.id === (event as OptionItem)?.value) ?? undefined;
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

  const handleBlur = async (cell: any) => {
    const result =
      !folderContentModel ||
      cell.original.content!.topics![0].id !== folderContentModel.content!.topics![0].id ||
      cell.original.content!.topics![0].score !== folderContentModel.content!.topics![0].score;
    if (result) return;

    await handleSubmit(folderContentModel);
  };

  const topicOptions = getSortableOptions(topics, undefined, [
    new OptionItem('[Not Applicable]', 0),
  ]);

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
            options={topicOptions}
            isDisabled={loading}
            isClearable={false}
            value={topicOptions?.find(
              (o) =>
                o.value ===
                (cell.original.content!.topics!.length > 0
                  ? cell.original.content!.topics![0].id
                  : 0),
            )}
            width={FieldSize.Medium}
            onChange={async (e: any) => await handleTopicChange(e, cell)}
            onBlur={async () => {
              if (
                !folderContentModel ||
                cell.original.content!.topics![0].id === folderContentModel.content!.topics![0].id
              )
                return;
              await handleSubmit(folderContentModel);
            }}
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
              onBlur={async () => await handleBlur(cell)}
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
    // {
    //   label: '',
    //   accessor: 'actions',
    //   width: '.25',
    //   cell: (cell) => (
    //     <Button
    //       variant={ButtonVariant.action}
    //       onClick={() => handleSave(folderContentModel)}
    //       disabled={loading}
    //       title="Save"
    //     >
    //       <FaSave className="indicator" />
    //     </Button>
    //   ),
    // },
  ];

  return result;
};
