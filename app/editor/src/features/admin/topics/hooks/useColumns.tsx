import React from 'react';
import { FaTrash } from 'react-icons/fa';
import {
  Button,
  ButtonVariant,
  Col,
  ITableHookColumn,
  ITopicModel,
  Text,
  ToggleGroup,
  TopicTypeName,
} from 'tno-core';

export const useColumns = (
  onClick: (event: any) => {},
  handleSubmit: (values: ITopicModel) => Promise<void>,
  loading: boolean,
): ITableHookColumn<ITopicModel>[] => {
  const [topicModel, setTopicModel] = React.useState<ITopicModel>();

  const handleChange = async (event: any, cell: any) => {
    const updatedTopic = { ...cell.original, name: event.target.value };
    setTopicModel(updatedTopic);
  };

  const handleBlur = async (cell: any) => {
    const result =
      !topicModel || cell.original.id !== topicModel.id || cell.original.name === topicModel.name;
    if (result) return;

    await handleSubmit(topicModel);
  };

  const result: ITableHookColumn<ITopicModel>[] = [
    {
      label: 'Topic Name',
      accessor: 'name',
      width: 3,
      cell: (cell) => {
        return (
          <Col flex="1">
            <Text
              name="name"
              disabled={loading}
              value={
                topicModel && topicModel.id === cell.original.id
                  ? topicModel.name
                  : cell.original.name
              }
              onBlur={async () => await handleBlur(cell)}
              onChange={async (e: any) => {
                // because we update the related TopicModel with every change
                // and the input value is bound to that model, we need to store
                // the cursor position else it jumps to the end of the input box
                // after each change.
                e.persist();
                const caretStart = e.target.selectionStart;
                const caretEnd = e.target.selectionEnd;
                // update the state
                await handleChange(e, cell);
                // reset the caret
                e.target.setSelectionRange(caretStart, caretEnd);
              }}
            />
          </Col>
        );
      },
    },
    {
      label: 'Type',
      accessor: 'topicType',
      width: 1,
      cell: (cell) => {
        return (
          <ToggleGroup
            className="inline-topic-type-toggle-group"
            defaultSelected={cell.original.topicType}
            options={Object.values(TopicTypeName).map((i) => ({
              id: i,
              label: i,
              data: i,
              onClick: () => {
                const updatedTopic = { ...cell.original, topicType: i } as ITopicModel;
                setTopicModel(updatedTopic);
              },
            }))}
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
      accessor: 'Remove',
      width: '80px',
      cell: (cell) => (
        <>
          <Button
            variant={ButtonVariant.danger}
            onClick={() => onClick(cell.original.id)}
            disabled={loading}
            title="Remove"
          >
            <FaTrash className="indicator" />
          </Button>
        </>
      ),
    },
  ];

  return result;
};
