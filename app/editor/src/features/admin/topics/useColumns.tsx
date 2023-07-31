import React from 'react';
import {
  Button,
  ButtonVariant,
  FieldSize,
  getEnumStringOptions,
  ITableHookColumn,
  ITopicModel,
  OptionItem,
  Select,
  TopicTypeName,
} from 'tno-core';

export const useColumns = (
  onClick: (event: any) => {},
  handleSubmit: (values: ITopicModel) => Promise<void>,
): ITableHookColumn<ITopicModel>[] => {
  const [topicModel, setTopicModel] = React.useState<ITopicModel>();

  const handleChange = async (event: any, cell: any) => {
    if (event.target.value !== cell.original.name) {
      const updatedTopic = { ...cell.original, name: event.target.value };
      setTopicModel(updatedTopic);
    }
  };

  const handleBlur = async (cell: any) => {
    const result =
      !topicModel || cell.original.id !== topicModel.id || cell.original.name === topicModel.name;
    if (result) return;

    await handleSubmit(topicModel);
  };

  const topicTypeOptions = getEnumStringOptions(TopicTypeName);

  const result: ITableHookColumn<ITopicModel>[] = [
    {
      label: 'Name',
      name: 'name',
      width: 1,
      cell: (cell) => {
        return (
          <input
            type="text"
            title="topic name"
            value={
              topicModel && topicModel.id === cell.original.id
                ? topicModel.name
                : cell.original.name
            }
            onBlur={async () => await handleBlur(cell)}
            onChange={async (e: any) => await handleChange(e, cell)}
            style={{ height: 35, width: '100%' }}
          />
        );
      },
    },
    {
      label: 'Type',
      name: 'topicType',
      width: 1,
      cell: (cell) => {
        return (
          <Select
            name="topicType"
            options={topicTypeOptions}
            value={topicTypeOptions?.find(
              (o) =>
                o.value ===
                (topicModel && topicModel.id === cell.original.id
                  ? topicModel.topicType
                  : cell.original.topicType),
            )}
            width={FieldSize.Medium}
            onChange={async (e) => {
              const option = topicTypeOptions.find((x) => x.value === (e as OptionItem)?.value);
              const updatedTopic = { ...cell.original, topicType: option?.value } as ITopicModel;
              setTopicModel(updatedTopic);
            }}
            onBlur={async () => {
              if (
                !topicModel ||
                cell.original.id !== topicModel.id ||
                cell.original.topicType === topicModel.topicType
              )
                return;

              await handleSubmit(topicModel);
            }}
          />
        );
      },
    },
    {
      label: '',
      name: 'Remove',
      width: '1',
      hAlign: 'center',
      cell: (cell) => (
        <>
          <Button variant={ButtonVariant.danger} onClick={() => onClick(cell.original.id)}>
            Remove
          </Button>
        </>
      ),
    },
  ];

  return result;
};
