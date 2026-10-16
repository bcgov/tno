import { useFormikContext } from 'formik';
import React from 'react';
import { useLookup } from 'store/hooks';
import { FlexboxTable, type IIngestModel } from 'tno-core';

import { columns, type ISourceMappingItem } from './constants/columns';
import * as styled from './styled';

/**
 * Maps the publication names in a feed to source codes.
 * Stored in 'configuration.sources' as 'name=code&name=code'.
 */
export const SourceMapping: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();
  const [lookups] = useLookup();

  const sources = React.useMemo(
    () =>
      lookups.sources
        .filter((s) => s.isEnabled)
        .map((s) => ({
          label: `${s.code} - ${s.name}`,
          value: s.code,
          search: [s.code, s.name, s.shortName].filter((v) => !!v).join(' '),
        })),
    [lookups.sources],
  );

  const initialItems = React.useCallback(() => {
    const data: ISourceMappingItem[] = [];
    let id = 0;
    const keyValues = values.configuration.sources?.split('&') ?? [];
    keyValues.forEach((x: string) => {
      data.push({ id: ++id, name: x.split('=')[0], source: x.split('=')[1] });
    });
    data.push({ id: ++id, name: '', source: '' });
    return data;
  }, [values.configuration.sources]);

  const [items, setItems] = React.useState<ISourceMappingItem[]>(initialItems());

  const updateItems = React.useCallback(
    (updatedItems: ISourceMappingItem[]) => {
      if (updatedItems.filter((x) => !x.name && !x.source).length === 0) {
        updatedItems.push({
          id: Math.max(0, ...updatedItems.map((x) => x.id)) + 1,
          name: '',
          source: '',
        });
      }
      setItems(updatedItems);
      setFieldValue(
        'configuration.sources',
        updatedItems
          .filter((t) => !!t.name)
          .map((item) => `${item.name?.trim()}=${item.source?.trim() ?? ''}`)
          .join('&'),
      );
    },
    [setFieldValue],
  );

  const handleRemove = (id: number) => {
    updateItems(items.filter((i) => i.id !== id));
  };

  const handleChange = (id: number, field: 'name' | 'source', value: string) => {
    updateItems(items.map((item) => (item.id === id ? { ...item, [field]: value } : item)));
  };

  return (
    <styled.ScrollTable>
      <b>Source Mapping</b>
      <FlexboxTable
        rowId="id"
        data={items}
        columns={columns(handleRemove, handleChange, sources)}
        showSort={true}
        showActive={false}
        pagingEnabled={false}
      />
    </styled.ScrollTable>
  );
};
