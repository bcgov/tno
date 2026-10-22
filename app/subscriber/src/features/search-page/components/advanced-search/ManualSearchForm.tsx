import React from 'react';
import { IoIosCog } from 'react-icons/io';
import { useApp } from 'store/hooks';
import {
  Claim,
  Col,
  getCookie,
  type IFilterAIManualFilterProps,
  RadioGroup,
  Row,
  TextArea,
} from 'tno-core';

import { AdvancedSearchSections, ExpandableRow, MoreOptions, SearchInGroup } from './components';
import { displayFiltersAsDropdownCookieKey, SearchOperatorOptions } from './constants';
import { SearchSettingsBinding, SearchSettingsContext } from './SearchSettingsContext';
import * as styled from './styled';

/**
 * The advanced search form bound to the filter prompt workbench's settings rather than the
 * search page's: the same sections, editing the settings the workbench is working on.
 */
export const ManualSearchForm: React.FC<IFilterAIManualFilterProps> = ({ settings, onChange }) => {
  const [{ userInfo }] = useApp();
  const isAdmin = !!userInfo?.roles.includes(Claim.administrator);
  const displayFiltersAsDropdown = !!getCookie(displayFiltersAsDropdownCookieKey);
  const binding = React.useMemo<SearchSettingsBinding>(
    () => [settings, onChange],
    [settings, onChange],
  );

  return (
    <SearchSettingsContext.Provider value={binding}>
      <styled.AdvancedSearch expanded className="manual-search-form">
        <Col className="manual-search-content" flex="1">
          <Row className="search-for-row">
            <label className="search-in-label" htmlFor="manualSearchText">
              Search for:
            </label>
          </Row>
          <TextArea
            id="manualSearchText"
            name="search"
            rows={3}
            value={settings.search ?? ''}
            onChange={(e) => onChange({ ...settings, search: e.target.value })}
          />
          <Row className="search-for-row">
            <Col className="text-area-container space-top">
              <SearchInGroup />
            </Col>
          </Row>
          <Row alignItems="center" gap="1rem" className="space-top">
            Default operator:
            <div className="radio-group">
              <RadioGroup
                name="manualSearchOperator"
                value={
                  SearchOperatorOptions[
                    settings.defaultOperator as keyof typeof SearchOperatorOptions
                  ] ?? SearchOperatorOptions.and
                }
                onChange={(val) =>
                  onChange({
                    ...settings,
                    defaultOperator: val.target.value.toLocaleLowerCase() as 'and' | 'or',
                  })
                }
                options={[SearchOperatorOptions.and, SearchOperatorOptions.or]}
                required={true}
                direction={'row'}
              />
            </div>
          </Row>
          <Col className="section top-spacer">
            <label className="search-in-label">Narrow your results by: </label>
            <AdvancedSearchSections
              displayFiltersAsDropdown={displayFiltersAsDropdown}
              isAdmin={isAdmin}
            />
          </Col>
          <Col className="section top-spacer">
            <label className="search-in-label">Display options:</label>
            <ExpandableRow
              icon={<IoIosCog />}
              title="Search result options"
              hasValues={
                !settings.searchUnpublished ||
                !!settings.boldKeywords ||
                (settings.actions?.length ?? 0) > 0 ||
                settings.size !== 500
              }
            >
              <MoreOptions />
            </ExpandableRow>
          </Col>
        </Col>
      </styled.AdvancedSearch>
    </SearchSettingsContext.Provider>
  );
};
