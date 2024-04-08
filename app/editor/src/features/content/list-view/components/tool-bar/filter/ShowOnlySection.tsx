import { showOnlyOptions, ShowOnlyValues } from 'features/content/constants';
import { IContentListFilter } from 'features/content/interfaces';
import React from 'react';
import { FaEye } from 'react-icons/fa';
import { ActionDelegate } from 'store';
import { useContent } from 'store/hooks';
import {
  Checkbox,
  Col,
  ContentTypeName,
  FieldSize,
  IOptionItem,
  replaceQueryParams,
  Row,
  Select,
  Show,
  ToolBarSection,
} from 'tno-core';

import { InputOption } from './InputOption';

export interface IShowOnlySectionProps {}

/**
 * Show only section
 * @param onChange determines what happens when the checkbox is checked or unchecked
 * @returns A section with checkboxes to show only certain content types
 */
export const ShowOnlySection: React.FC<IShowOnlySectionProps> = () => {
  const [{ filter, filterAdvanced }, { storeFilter }] = useContent();

  const [width, setWidth] = React.useState(window.innerWidth);

  // recalculates the width of the screen when the screen is resized
  React.useEffect(() => {
    if (typeof window === 'undefined') {
      return;
    }

    const calcWidth = () => {
      setWidth(window.innerWidth);
    };

    window.addEventListener('resize', calcWidth);

    // This is likely unnecessary, as the initial state should capture
    // the size, however if a resize occurs between initial state set by
    // React and before the event listener is attached, this
    // will just make sure it captures that.
    calcWidth();

    // Return a function to disconnect the event listener
    return () => window.removeEventListener('resize', calcWidth);
  }, []);

  const selectedValues = React.useMemo(() => {
    const getSelectedOptions = (filter: IContentListFilter) => {
      const selectedOptions: IOptionItem[] = [];
      if (filter.contentTypes?.includes(ContentTypeName.PrintContent))
        selectedOptions.push(showOnlyOptions[0]);
      if (filter.hasTopic) selectedOptions.push(showOnlyOptions[1]);
      if (filter.commentary) selectedOptions.push(showOnlyOptions[2]);
      if (filter.topStory) selectedOptions.push(showOnlyOptions[3]);
      if (filter.onlyPublished) selectedOptions.push(showOnlyOptions[4]);
      if (filter.pendingTranscript) selectedOptions.push(showOnlyOptions[5]);
      return selectedOptions;
    };

    return getSelectedOptions(filter);
  }, [filter]);

  const onChange = React.useCallback(
    (action: ActionDelegate<IContentListFilter>) => {
      storeFilter((filter) => {
        var result = typeof action === 'function' ? action(filter) : action;
        replaceQueryParams({ ...result, ...filterAdvanced }, { includeEmpty: false });
        return result;
      });
    },
    [filterAdvanced, storeFilter],
  );

  return (
    <ToolBarSection
      children={
        <Col>
          <Show visible={width <= 1847}>
            <Select
              className="select"
              name="showOnly"
              placeholder="Show Only"
              isMulti
              closeMenuOnSelect={false}
              hideSelectedOptions={false}
              options={showOnlyOptions}
              width={FieldSize.Small}
              defaultValue={selectedValues}
              components={{
                Option: InputOption,
              }}
              onChange={(newValues) => {
                const values = newValues as IOptionItem[];
                onChange((filter) => {
                  return {
                    ...filter,
                    pageIndex: 0,
                    contentTypes: values.some((o) => o.value === ShowOnlyValues.PrintContent)
                      ? !filter.contentTypes?.includes(ContentTypeName.PrintContent)
                        ? [...filter.contentTypes, ContentTypeName.PrintContent]
                        : [...filter.contentTypes]
                      : filter.contentTypes.filter((x) => x !== ContentTypeName.PrintContent),
                    hasTopic: values.some((o) => o.value === ShowOnlyValues.HasTopic),
                    commentary: values.some((o) => o.value === ShowOnlyValues.Commentary),
                    topStory: values.some((o) => o.value === ShowOnlyValues.TopStory),
                    onlyPublished: values.some((o) => o.value === ShowOnlyValues.Published),
                    pendingTranscript: values.some(
                      (o) => o.value === ShowOnlyValues.PendingTranscript,
                    ),
                  };
                });
              }}
            />
          </Show>
          <Show visible={width > 1847}>
            <Row>
              <Checkbox
                name="isPrintContent"
                label="Print Content"
                tooltip="Newspaper content without audio/video"
                checked={filter.contentTypes?.includes(ContentTypeName.PrintContent) === true}
                onChange={(e) => {
                  onChange({
                    ...filter,
                    pageIndex: 0,
                    contentTypes: e.target.checked ? [ContentTypeName.PrintContent] : [],
                  });
                }}
              />
              <Checkbox
                className="spaced"
                name="hasTopic"
                label="Has Topic"
                tooltip="Content included in Event of the Day"
                value={filter.hasTopic}
                checked={filter.hasTopic}
                onChange={(e) => {
                  onChange({
                    ...filter,
                    pageIndex: 0,
                    hasTopic: e.target.checked,
                  });
                }}
              />
              <Checkbox
                className="spaced"
                name="pendingTranscript"
                label="Pending Transcript"
                tooltip="Pending Transcript"
                checked={filter.pendingTranscript ?? false}
                onChange={(e) => {
                  onChange({
                    ...filter,
                    pageIndex: 0,
                    pendingTranscript: e.target.checked,
                  });
                }}
              />
            </Row>
            <Row>
              <Checkbox
                name="commentary"
                label="Commentary"
                tooltip="Content identified as commentary"
                checked={filter.commentary}
                onChange={(e) => {
                  onChange({
                    ...filter,
                    pageIndex: 0,
                    commentary: e.target.checked,
                  });
                }}
              />
              <Checkbox
                className="spaced"
                name="topStory"
                label="Top Story"
                tooltip="Content identified as a top story"
                checked={filter.topStory}
                onChange={(e) => {
                  onChange({
                    ...filter,
                    pageIndex: 0,
                    topStory: e.target.checked,
                  });
                }}
              />
              <Checkbox
                className="spaced"
                name="onlyPublished"
                label="Published"
                tooltip="Published Content"
                checked={filter.onlyPublished}
                onChange={(e) => {
                  onChange({
                    ...filter,
                    pageIndex: 0,
                    onlyPublished: e.target.checked,
                  });
                }}
              />
            </Row>
          </Show>
        </Col>
      }
      label="SHOW ONLY"
      icon={<FaEye />}
    />
  );
};
