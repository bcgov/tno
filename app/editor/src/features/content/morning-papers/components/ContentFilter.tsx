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

import { defaultSources } from '../constants';
import { IMorningReportsFilter } from '../interfaces';
import * as styled from './styled';

export interface IContentFilter {
  /** The current filter values. */
  filter: IMorningReportsFilter;
  /** Event when filter changes. */
  onFilterChange: (filter: IMorningReportsFilter) => void;
}

/**
 * Provides a component to filter content on the morning reports.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ContentFilter: React.FC<IContentFilter> = ({ onFilterChange, filter }) => {
  const [{ productOptions, sourceOptions, sources }] = useLookupOptions();

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
                      includeHidden: false,
                      onlyHidden: false,
                    }),
                },
                {
                  label: 'PREVIEW A.M. REPORT',
                  onClick: () =>
                    onFilterChange({
                      ...filter,
                      onlyPublished: true,
                      includeHidden: false,
                      onlyHidden: false,
                    }),
                },
                {
                  label: 'SHOW HIDDEN ONLY',
                  onClick: (e) => {
                    onFilterChange({
                      ...filter,
                      pageIndex: 0,
                      onlyPublished: false,
                      includeHidden: true,
                      onlyHidden: true,
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
                name="productIds"
                className="select"
                placeholder="Product"
                isMulti
                closeMenuOnSelect={false}
                hideSelectedOptions={false}
                options={filterEnabledOptions(productOptions)}
                components={{
                  Option: InputOption,
                }}
                onChange={(newValues) => {
                  const productIds = Array.isArray(newValues)
                    ? newValues.map((opt) => opt.value)
                    : [0];
                  onFilterChange({
                    ...filter,
                    pageIndex: 0,
                    productIds: productIds,
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
                  sourceIds: defaultSources(sources).map((s) => s.id),
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
