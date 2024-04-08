import { IContentListFilter } from 'features/content/interfaces';
import { InputOption } from 'features/content/list-view/components/tool-bar/filter';
import React from 'react';
import { FaEye, FaFilter, FaIcons, FaNewspaper, FaWindowRestore } from 'react-icons/fa';
import { useLookupOptions } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  filterEnabledOptions,
  Row,
  Select,
  ToggleGroup,
} from 'tno-core';

import { usePaperSources } from '../hooks';
import * as styled from './styled';

export interface IContentFilter {
  /** The current filter values. */
  filter: IContentListFilter;
  /** Event when filter changes. */
  onFilterChange: (filter: IContentListFilter) => void;
}

/**
 * Provides a component to filter content on the papers.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ContentFilter: React.FC<IContentFilter> = ({ onFilterChange, filter }) => {
  const [{ mediaTypeOptions, sourceOptions }] = useLookupOptions();
  const paperSources = usePaperSources();

  return (
    <styled.ContentFilter label="FILTER CONTENT" icon={<FaFilter />}>
      <Col className="filters">
        <Row>
          <Row>
            <FaEye className="icon-indicator" />
            <ToggleGroup
              defaultSelected="ALL TODAY'S CONTENT"
              options={[
                {
                  label: "ALL TODAY'S CONTENT",
                  onClick: () =>
                    onFilterChange({
                      ...filter,
                      onlyPublished: false,
                      isHidden: false,
                      topStory: false,
                      commentary: false,
                      featuredStory: false,
                    }),
                },
                {
                  label: 'SEE SHORTLIST',
                  onClick: () =>
                    onFilterChange({
                      ...filter,
                      onlyPublished: false,
                      isHidden: false,
                      topStory: true,
                      commentary: true,
                      featuredStory: true,
                    }),
                },
                {
                  label: 'SEE HIDDEN ONLY',
                  onClick: (e) => {
                    onFilterChange({
                      ...filter,
                      pageIndex: 0,
                      onlyPublished: false,
                      isHidden: true,
                      topStory: false,
                      commentary: false,
                      featuredStory: false,
                    });
                  },
                },
              ]}
            />
          </Row>
          <Row>
            <FaIcons className="icon-indicator" height="2em" width="2em" />
            <Col>
              <Select
                name="mediaTypeIds"
                className="select"
                placeholder="Media Type"
                isMulti
                closeMenuOnSelect={false}
                hideSelectedOptions={false}
                options={filterEnabledOptions(mediaTypeOptions)}
                components={{
                  Option: InputOption,
                }}
                onChange={(newValues) => {
                  const mediaTypeIds = Array.isArray(newValues)
                    ? newValues.map((opt) => opt.value)
                    : [0];
                  onFilterChange({
                    ...filter,
                    pageIndex: 0,
                    mediaTypeIds: mediaTypeIds,
                  });
                }}
              />
            </Col>
          </Row>
        </Row>

        <Row>
          <FaNewspaper className="icon-indicator" />
          <Col flex="1 1 auto">
            <Select
              name="sourceIds"
              className="select sources"
              isMulti
              hideSelectedOptions={false}
              placeholder="Sources"
              options={sourceOptions}
              value={sourceOptions.filter((opt) => filter.sourceIds?.includes(Number(opt.value)))}
              onChange={(newValues) => {
                const sourceIds = Array.isArray(newValues)
                  ? newValues.map((opt) => opt.value)
                  : [0];
                onFilterChange({
                  ...filter,
                  pageIndex: 0,
                  sourceIds: sourceIds,
                });
              }}
            />
          </Col>
          <Col justifyContent="center">
            <Button
              variant={ButtonVariant.link}
              tooltip="Default Sources"
              onClick={() => {
                onFilterChange({
                  ...filter,
                  sourceIds: paperSources.map((s) => s.id),
                });
              }}
            >
              <FaWindowRestore />
            </Button>
          </Col>
        </Row>
      </Col>
    </styled.ContentFilter>
  );
};
