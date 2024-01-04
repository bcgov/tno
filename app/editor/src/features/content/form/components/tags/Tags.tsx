import { IContentForm } from 'features/content/form/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaListAlt } from 'react-icons/fa';
import { useLookup } from 'store/hooks';
import { Button, Col, FieldSize, IOptionItem, ITagModel, Row, Select } from 'tno-core';

import { DraggableTagList } from './DraggableTagList';
import * as styled from './styled';

export interface ITagsProps {
  selectedExternal?: string[];
}

/**
 * The component that renders tags for a given text field
 * @returns the Tags component
 */
export const Tags: React.FC<ITagsProps> = ({ selectedExternal }) => {
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const [{ tags }] = useLookup();
  const [showList, setShowList] = React.useState(false);
  const [externalTags, setExternalTags] = React.useState<ITagModel[]>([]);
  const [manualTags, setManualTags] = React.useState<ITagModel[]>([]);

  /** ensure table is in view depending on where user has scrolled to. */
  React.useEffect(() => {
    if (document.getElementById('tag-list')) {
      document.getElementById('tag-list')?.scrollIntoView({ behavior: 'smooth' });
    }
  }, [showList]);

  React.useEffect(() => {
    const ext = tags.filter((tag) => selectedExternal?.some((t: string) => t === tag.code));
    setExternalTags(ext);
    const newTags = ext.concat(manualTags);
    setFieldValue('tags', newTags);

    // eslint-disable-next-line no-console
    console.log('tagz', ext, manualTags);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedExternal]);

  /** convert tags to IOptionItem format */
  const tagOptions: IOptionItem[] = tags.map((tag) => {
    return {
      label: tag.code,
      value: tag.id,
      isDisabled: !tag.isEnabled,
    } as IOptionItem;
  });

  /** prepare tags to proper format for the API */
  const convertTags = (selectedTags: IOptionItem[]) => {
    return tags
      .filter((tag) => selectedTags.some((t: IOptionItem) => t.value === tag.id))
      .map((tag) => tag);
  };

  const addManualTags = (selectedTags: any) => {
    const manuallySelectedTags = selectedTags.filter((tag: IOptionItem) =>
      externalTags?.some((t: ITagModel) => t.code !== tag.label),
    );
    setManualTags(convertTags(manuallySelectedTags as IOptionItem[]));
    const newTags = manuallySelectedTags.concat(externalTags);
    // eslint-disable-next-line no-console
    console.log('tagz', externalTags, manuallySelectedTags);
    setFieldValue('tags', newTags);
  };
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
            onChange={(selectedTags) => addManualTags(selectedTags)}
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
