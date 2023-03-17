import { ToolBarSection } from 'components/tool-bar';
import { showOnlyOptions } from 'features/content/form/constants/showOnlyOptions';
import { IContentListFilter } from 'features/content/list-view/interfaces';
import { ContentTypeName } from 'hooks';
import React from 'react';
import { FaEye } from 'react-icons/fa';
import { ActionDelegate } from 'store';
import { useContent } from 'store/hooks';
import { Checkbox, Col, FieldSize, replaceQueryParams, Row, Select, Show } from 'tno-core';

import { useShowOnlyContentType } from './hooks';
import { InputOption } from './InputOption';
import { getSelectedOptions } from './utils';

export interface IShowOnlySectionProps {}

/**
 * Show only section
 * @param onChange determines what happens when the checkbox is checked or unchecked
 * @returns A section with checkboxes to show only certain content types
 */
export const ShowOnlySection: React.FC<IShowOnlySectionProps> = () => {
  const [{ filter, filterAdvanced }, { storeFilter }] = useContent();
  const showContentType = useShowOnlyContentType();

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
              defaultValue={getSelectedOptions(filter)}
              components={{
                Option: InputOption,
              }}
              onChange={(newValues: any, { action }) => {
                if (!newValues.length)
                  onChange({
                    ...filter,
                    showOnly: '',
                    onTicker: '',
                    commentary: '',
                    topStory: '',
                    includedInTopic: false,
                    contentType: undefined,
                  });
                else {
                  showContentType(newValues);
                }
              }}
            />
          </Show>
          <Show visible={width > 1847}>
            <Row>
              <Checkbox
                name="isPrintContent"
                label="Print Content"
                tooltip="Newspaper content without audio/video"
                checked={filter.contentType === ContentTypeName.PrintContent}
                onChange={(e) => {
                  onChange({
                    ...filter,
                    pageIndex: 0,
                    contentType: e.target.checked ? ContentTypeName.PrintContent : undefined,
                  });
                }}
              />
              <Checkbox
                className="spaced"
                name="includedInTopic"
                label="Included in EoD"
                tooltip="Content included in Event of the Day"
                value={filter.includedInTopic}
                checked={filter.includedInTopic}
                onChange={(e) => {
                  onChange({
                    ...filter,
                    pageIndex: 0,
                    includedInTopic: e.target.checked,
                  });
                }}
              />
            </Row>
            <Row>
              <Checkbox
                name="commentary"
                label="Commentary"
                value="Commentary"
                tooltip="Content identified as commentary"
                checked={filter.commentary !== ''}
                onChange={(e) => {
                  onChange({
                    ...filter,
                    pageIndex: 0,
                    commentary: e.target.checked ? e.target.value : '',
                  });
                }}
              />
              <Checkbox
                className="spaced"
                name="topStory"
                label="Top Story"
                value="Top Story"
                tooltip="Content identified as a top story"
                checked={filter.topStory !== ''}
                onChange={(e) => {
                  onChange({
                    ...filter,
                    pageIndex: 0,
                    topStory: e.target.checked ? e.target.value : '',
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
