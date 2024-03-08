import { IContentForm } from 'features/content/form/interfaces';
import { useFormikContext } from 'formik';
import _ from 'lodash';
import React from 'react';
import { FaListAlt } from 'react-icons/fa';
import { useLookup } from 'store/hooks';
import { Button, Col, FieldSize, IOptionItem, Row, Select } from 'tno-core';

import { DraggableTagList } from './DraggableTagList';
import * as styled from './styled';

export interface ITagsProps {
  defaultTags?: string[];
}

/**
 * The component that renders tags for a given text field
 * @returns the Tags component
 */
export const Tags: React.FC<ITagsProps> = ({ defaultTags }) => {
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const [{ tags }] = useLookup();

  const [showList, setShowList] = React.useState(false);
  const [tagOptions, setTagOptions] = React.useState(
    tags.map((tag) => {
      return {
        label: tag.code,
        value: tag.id,
        isDisabled: !tag.isEnabled,
      } as IOptionItem;
    }),
  );

  React.useEffect(() => {
    setTagOptions(
      tags.map((tag) => {
        return {
          label: tag.code,
          value: tag.id,
          isDisabled: !tag.isEnabled,
        } as IOptionItem;
      }),
    );
  }, [tags]);

  /** ensure table is in view depending on where user has scrolled to. */
  React.useEffect(() => {
    if (document.getElementById('tag-list')) {
      document.getElementById('tag-list')?.scrollIntoView({ behavior: 'smooth' });
    }
  }, [showList]);

  React.useEffect(() => {
    const initTags = tags.filter((tag) => defaultTags?.some((code) => code === tag.code));
    const newTags = _.uniqBy(values.tags.concat(initTags), (tag) => tag.code);
    setFieldValue('tags', newTags);
    // Only update if the default values have changed.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [defaultTags]);

  const addTags = React.useCallback(
    (selectedTags: any) => {
      const newTags = tags
        .filter((tag) => selectedTags.some((t: IOptionItem) => t.value === tag.id))
        .map((tag) => tag);
      setFieldValue('tags', newTags);
    },
    [setFieldValue, tags],
  );

  return (
    <styled.Tags className="multi-group">
      <DraggableTagList showList={showList} setShowList={setShowList} />
      <Col>
        <Row>
          <label>Tags</label>
        </Row>
        <Row>
          <Select
            isMulti
            width={FieldSize.Big}
            name="tags"
            options={tagOptions}
            maxMenuHeight={120}
            menuPlacement="top"
            value={tagOptions.filter((option) =>
              values.tags.find((tag) => tag.id === option.value),
            )}
            onChange={(selectedTags) => addTags(selectedTags)}
          />
          <Button
            tooltip="Show tag list"
            onClick={() => {
              setShowList(!showList);
            }}
          >
            <FaListAlt />
          </Button>
        </Row>
      </Col>
    </styled.Tags>
  );
};
