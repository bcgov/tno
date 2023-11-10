import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { DateFilter } from 'components/date-filter';
import { determineColumns } from 'features/home/constants';
import { createFilterSettings } from 'features/utils';
import moment from 'moment';
import React from 'react';
import { FiRefreshCcw } from 'react-icons/fi';
import { useNavigate } from 'react-router-dom';
import { useContent, useLookup, useLookupOptions } from 'store/hooks';
import {
  FieldSize,
  FlexboxTable,
  generateQuery,
  IContentModel,
  IFilterSettingsModel,
  IOptionItem,
  Row,
  Select,
} from 'tno-core';

import * as styled from './styled';

export const FilterMedia: React.FC = () => {
  const navigate = useNavigate();
  const [{ filterAdvanced }, { findContentWithElasticsearch }] = useContent();
  const [{ productOptions, sourceOptions }] = useLookupOptions();
  const [{ sources, products }] = useLookup();
  const [results, setResults] = React.useState<IContentModel[]>([]);

  const [productValue, setProductValue] = React.useState<IOptionItem | null>();
  const [sourceValue, setSourceValue] = React.useState<IOptionItem | null>();

  const [settings, setSettings] = React.useState<IFilterSettingsModel>(
    createFilterSettings(
      filterAdvanced.startDate ?? moment().startOf('day').toISOString(),
      filterAdvanced.endDate ?? moment().endOf('day').toISOString(),
    ),
  );

  const fetchResults = React.useCallback(
    async (filter: MsearchMultisearchBody) => {
      try {
        const res: any = await findContentWithElasticsearch(filter, false);
        setResults(res.hits.hits.map((h: { _source: IContentModel }) => h._source));
      } catch {}
    },
    [findContentWithElasticsearch],
  );

  React.useEffect(() => {
    setSettings(
      createFilterSettings(
        filterAdvanced.startDate ?? moment().startOf('day').toISOString(),
        filterAdvanced.endDate ?? moment().endOf('day').toISOString(),
      ),
    );
  }, [filterAdvanced?.startDate, filterAdvanced?.endDate]);

  return (
    <styled.FilterMedia>
      <Row className="tool-bar">
        <Select
          width={FieldSize.Medium}
          name="select-product"
          placeholder={'Select a media type'}
          isClearable={false}
          clearValue={() => setProductValue(null)}
          value={productValue}
          onChange={(e: any) => {
            if (!!e.value) {
              setProductValue(e);
            }
          }}
          options={productOptions}
        />
        <Select
          value={sourceValue}
          isClearable={false}
          options={sourceOptions}
          placeholder={'Select a source'}
          clearValue={() => setSourceValue(null)}
          onChange={(e: any) => {
            if (!!e.value) {
              setSourceValue(e);
            }
          }}
          name="source-select"
          width={FieldSize.Medium}
        />
        <DateFilter />
        <FiRefreshCcw
          className="reset"
          onClick={() => {
            fetchResults(
              generateQuery({
                ...settings,
                defaultSearchOperator: 'and',
                productIds: [products.find((p) => p.name === productValue?.label)?.id ?? 0],
                sourceIds: [sources.find((s) => s.id === sourceValue?.value)?.id ?? 0],
              }),
            );
          }}
        />
      </Row>
      <Row className="table-container">
        <FlexboxTable
          rowId="id"
          columns={determineColumns('all')}
          groupBy={(item) => item.original.source?.name ?? ''}
          onRowClick={(e: any) => {
            navigate(`/view/${e.original.id}`);
          }}
          data={results}
          pageButtons={5}
          showPaging={false}
        />
      </Row>
    </styled.FilterMedia>
  );
};
