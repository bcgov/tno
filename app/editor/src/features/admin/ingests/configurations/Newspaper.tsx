import { useFormikContext } from 'formik';
import React from 'react';
import { useLookup } from 'store/hooks';
import {
  Col,
  filterEnabledOptions,
  FlexboxTable,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  IIngestModel,
  OptionItem,
  Row,
} from 'tno-core';

import { FileTypes, Languages, TimeZones } from './constants';
import { columns } from './constants/columns';
import { ImportContent } from './ImportContent';
import * as styled from './styled';

export const Newspaper: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();

  const timeZone = TimeZones.find((t) => t.value === values.configuration.timeZone);
  const language = Languages.find((t) => t.value === values.configuration.language);
  const fileType = FileTypes.find((t) => t.value === values.configuration.fileFormat);
  const [lookups] = useLookup();
  const sources = lookups.sources.map((s) => new OptionItem(s.name, s.code, !s.isEnabled));
  const source = sources.find((t) => t.value === values.configuration.defaultSource);

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
      <Row>
        <Col flex="1">
          <FormikCheckbox label="Escape Content" name="configuration.escapeContent" />
          <FormikCheckbox label="Add Parent" name="configuration.addParent" />
          <FormikCheckbox label="Fix Blacks Newsgroup XML" name="configuration.fixBlacksXml" />
        </Col>
        <Col flex="1"></Col>
      </Row>
      <Row>
        <Col flex="1">
          <FormikSelect
            label="Timezone"
            name="configuration.timeZone"
            tooltip="Timezone of the source"
            options={TimeZones}
            value={timeZone}
          />
        </Col>
        <Col flex="1">
          <FormikText
            label="Date Offset"
            name="configuration.dateOffset"
            type="number"
            tooltip="Which day to parse (0 = today)"
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <FormikSelect
            label="File Type"
            name="configuration.fileFormat"
            tooltip="The file extension (xml, fms)"
            options={FileTypes}
            value={fileType}
            required
          />
        </Col>
        <Col flex="1">
          <FormikSelect
            label="Default Language"
            name="configuration.language"
            options={Languages}
            value={language}
            tooltip="The language of the content unless specified by the path below"
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <FormikText
            label="Path to Files"
            name="configuration.path"
            value={values.configuration.path}
          />
        </Col>
        <Col flex="1">
          <FormikText
            label="File Name Pattern"
            name="configuration.filePattern"
            tooltip="The filename expression"
          />
        </Col>
      </Row>
      <p>
        The following provide a path within the newspaper file to extract data for each content
        entry.
      </p>
      <Row>
        <Row flex="1">
          <Col flex="1">
            <FormikText label="ID" name="configuration.namespaces.0.id" />
          </Col>
          <Col flex="4">
            <FormikText label="Namespace" name="configuration.namespaces.0.href" />
          </Col>
        </Row>
        <Row flex="1">
          <Col flex="1">
            <FormikText label="ID" name="configuration.namespaces.1.id" />
          </Col>
          <Col flex="4">
            <FormikText label="Namespace" name="configuration.namespaces.1.href" />
          </Col>
        </Row>
      </Row>
      <Row>
        <Col flex="1">
          <FormikText
            label="Entry"
            name="configuration.item"
            tooltip="The path to the content item (i.e. story)"
            required
          />
        </Col>
        <Col flex="1">
          <FormikText
            label="UID"
            name="configuration.id"
            tooltip="The path to the content id"
            required
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <FormikText
            label="Published Date"
            name="configuration.date"
            tooltip="The path to the published date"
            required
          />
        </Col>
        <Col flex="1">
          <FormikText
            label="Date Format"
            name="configuration.dateFmt"
            tooltip="The format of the published date"
            placeholder="MM-dd-yyyy"
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <FormikText
            label="Headline"
            name="configuration.headline"
            tooltip="The path to the headline"
            required
          />
        </Col>
        <Col flex="1">
          <FormikText
            label="Summary"
            name="configuration.summary"
            tooltip="The path to the summary"
            required
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <FormikText
            label="Paper Name"
            name="configuration.papername"
            tooltip="The path to the paper name"
            required
          />
        </Col>
        <Col flex="1">
          <FormikText
            label="Content"
            name="configuration.story"
            tooltip="The path to the content body or full story"
            required
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <FormikText
            label="Section"
            name="configuration.section"
            tooltip="The path to the section"
          />
        </Col>
        <Col flex="1">
          <FormikText label="Page" name="configuration.page" tooltip="The path to the page" />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <FormikText label="Author" name="configuration.author" tooltip="The path to the author" />
        </Col>
        <Col flex="1" justifyContent="center">
          <FormikCheckbox
            label="Byline Title Case"
            tooltip="Applies title case when the text is ALL CAPS"
            name="configuration.bylineTitleCase"
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <FormikText label="Tags" name="configuration.tags" tooltip="The path to the tags" />
        </Col>
        <Col flex="1">
          <FormikText
            label="Language"
            name="configuration.lang"
            tooltip="The path to the language"
          />
        </Col>
      </Row>
      <Row>
        <Col className="frm-in">
          <label>Text Formatting</label>
          <FormikCheckbox
            label="Remove newlines"
            tooltip="Remove newline formatting in text"
            name="configuration.removeNewlines"
          />
        </Col>
      </Row>
      <b>Source Mapping</b>
      <FlexboxTable
        rowId="id"
        data={items}
        columns={columns(handleRemove, onChange)}
        showSort={true}
        showActive={false}
        pagingEnabled={false}
      />
      <FormikSelect
        label="Default Source"
        name="configuration.defaultSource"
        tooltip="Source to use for publications missing from Sources list. If not set, articles will be discarded"
        options={filterEnabledOptions(sources, source?.value)}
        value={source}
      />
    </styled.IngestType>
  );
};
