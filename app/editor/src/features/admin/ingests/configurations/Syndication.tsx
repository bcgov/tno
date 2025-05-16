import { useFormikContext } from 'formik';
import React from 'react';
import { FlexboxTable, FormikCheckbox, FormikSelect, FormikText, IIngestModel } from 'tno-core';

import { TimeZones } from './constants';
import { columns } from './constants/columns';
import { ImportContent } from './ImportContent';
import * as styled from './styled';

export const Syndication: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();
  const timeZone = TimeZones.find((t) => t.value === values.configuration.timeZone);

  const initialItems = React.useCallback(() => {
    const data: any[] = [];
    let id = 0;
    const keyValues = values.configuration.sources?.split('&') ?? [];
    keyValues.forEach((x: string) => {
      data.push({ id: ++id, name: x.split('=')[0], source: x.split('=')[1] });
    });
    data.push({ id: ++id, name: '', source: '' });
    return data;
  }, [values.configuration.sources]);

  const [items, setItems] = React.useState<{ id: number; name: string; source: string }[]>(
    initialItems(),
  );

  const updateItems = React.useCallback(
    (updatedItems: any[]) => {
      if (updatedItems.filter((x) => !x.name && !x.source).length === 0)
        updatedItems.push({ id: updatedItems.length + 1, name: '', source: '' });
      setItems(updatedItems);
      setFieldValue(
        'configuration.sources',
        updatedItems
          .filter((t) => !!t.name)
          .map((item) => `${item.name?.trim()}=${item.source?.trim()}`)
          .join('&'),
      );
    },
    [setFieldValue],
  );

  const handleRemove = async (id: number) => {
    const item = items.find((i) => i.id === id);
    if (!item) return;

    const updatedItems = items.filter((i) => i.id !== id);
    updateItems(updatedItems);
  };

  const onChange = (event: any, cell: any, isSource = true) => {
    const pointer = event.target.selectionStart;
    window.requestAnimationFrame(() => {
      event.target.selectionEnd = pointer;
    });
    const updatedItem = isSource
      ? { ...cell.original, source: event.target.value }
      : { ...cell.original, name: event.target.value };
    const updatedItems = items.map((item) => (item.id === cell.original.id ? updatedItem : item));
    updateItems(updatedItems);
  };

  return (
    <styled.IngestType>
      <ImportContent />
      <FormikText label="Syndication Feed URL" name="configuration.url" required />
      <FormikCheckbox
        label="Custom RSS/ATOM"
        name="configuration.customFeed"
        tooltip="If the feed doesn't follow the standardized rules it is a custom feed."
      />
      <FormikSelect
        label="Timezone"
        name="configuration.timeZone"
        tooltip="Timezone of the source"
        options={TimeZones}
        value={timeZone}
      />
      <FormikText
        label="Username"
        name="configuration.username"
        tooltip="Configure in the source connection"
      />
      <FormikText
        label="Password"
        name="configuration.password"
        tooltip="Configure in the source connection"
        type="password"
        autoComplete="off"
      />
      <FormikCheckbox
        label="Fetch Content Body Separately"
        name="configuration.fetchContent"
        tooltip="Whether content body is located remotely"
        onChange={(e) => {
          setFieldValue('configuration.fetchContent', e.currentTarget.checked);
        }}
      />
      <b>Source Mapping</b>
      <FlexboxTable
        rowId="id"
        data={items}
        columns={columns(handleRemove, onChange)}
        showSort={true}
        showActive={false}
        pagingEnabled={false}
      />
    </styled.IngestType>
  );
};
