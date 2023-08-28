import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { useLookupOptions } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  FormikDatePicker,
  FormikText,
  IFilterModel,
  OptionItem,
  Row,
  Select,
  SentimentSlider,
  ToggleGroup,
} from 'tno-core';

import { searchInOptions } from './constants/searchInOptions';
import { generateQuery, getActionOptions } from './utils';

/**
 * The page used to view and edit report filter.
 * @returns Component.
 */
export const FilterFormQuery: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IFilterModel>();
  const [{ productOptions, sourceOptions, seriesOptions, contributorOptions, actions }] =
    useLookupOptions();

  const [filter, setFilter] = React.useState(JSON.stringify(values.query, null, 2));
  const [actionOptions, setActionOptions] = React.useState(getActionOptions(actions));

  React.useEffect(() => {
    setActionOptions(getActionOptions(actions));
  }, [actions]);

  /**
   * Update the settings and query values based on the new key=value.
   */
  const updateQuery = React.useCallback(
    (key: string, value: any) => {
      var settings = { ...values.settings };
      settings[key] = value;
      if (key === 'dateOffset') {
        settings = { ...settings, startDate: undefined, endDate: undefined };
      } else if (key === 'startDate' || key === 'endDate') {
        settings = { ...settings, dateOffset: undefined };
      }
      const query = generateQuery(settings, values.query);
      setFieldValue('settings', settings);
      setFieldValue('query', query);
      setFilter(JSON.stringify(query, null, 2));
    },
    [setFieldValue, values.query, values.settings],
  );

  const searchOptions = searchInOptions((value) => updateQuery('searchIn', value));

  return (
    <>
      <Col>
        <h2>{values.name}</h2>
        <Row>
          <Col flex="1">
            <p>An Elasticsearch query provides a way to search for content.</p>
          </Col>
          <Button
            variant={ButtonVariant.secondary}
            onClick={() => {
              setFieldValue('settings', {});
              setFieldValue('query', {});
              setFilter('');
            }}
          >
            Clear Query
          </Button>
        </Row>
        <Row alignItems="center">
          <FormikText
            name="settings.size"
            label="Number of Stories"
            type="number"
            width="10ch"
            value={values.settings.size ?? 10}
            onChange={(e) => {
              const value = !!e.target.value ? parseInt(e.target.value) : 10;
              updateQuery('size', value);
            }}
          />
          <p>
            All filters must have a upward limit of content returned in a single request. The
            default limit is 10.
          </p>
        </Row>
        <Row>
          <FormikText
            name="settings.search"
            label="Keywords"
            value={values.settings.search ?? ''}
            onChange={(e) => {
              const value = e.target.value;
              updateQuery('search', value);
            }}
          >
            <ToggleGroup
              className="search-in"
              options={searchOptions}
              defaultSelected={values.settings.searchIn ?? searchOptions[0].label}
            />
          </FormikText>
        </Row>
        <Row nowrap>
          <Col>
            <FormikDatePicker
              name="settings.startDate"
              label="Start Date"
              value={values.settings.startDate ?? ''}
              width="13ch"
              onChange={(value) => {
                updateQuery('startDate', value);
              }}
              disabled={values.settings.dateOffset}
            />
          </Col>
          <Col>
            <FormikDatePicker
              name="settings.endDate"
              label="End Date"
              value={values.settings.endDate ?? ''}
              width="13ch"
              onChange={(value) => {
                updateQuery('endDate', value);
              }}
              disabled={values.settings.dateOffset}
            />
          </Col>
          <Row nowrap>
            <Col justifyContent="center" className="frm-in pad-05">
              <label>Or</label>
            </Col>
            <FormikText
              name="settings.dateOffset"
              label="Date Offset"
              type="number"
              width="10ch"
              value={values.settings.dateOffset ?? ''}
              onChange={(e) => {
                const value = !!e.target.value ? parseInt(e.target.value) : undefined;
                updateQuery('dateOffset', value);
              }}
              disabled={values.settings.startDate || values.settings.endDate}
            />
            <p>
              A date offset provides a consistent way to search for content that was published
              relative to today's date.
            </p>
          </Row>
        </Row>
        <Row>
          <Col flex="1">
            <Select
              name="sourceIds"
              label="Sources"
              isMulti
              options={sourceOptions}
              value={
                sourceOptions.filter((mt) =>
                  values.settings.sourceIds?.some((p: number) => p === mt.value),
                ) ?? []
              }
              onChange={(newValue: any) => {
                const sourceIds = newValue.map((v: OptionItem) => v.value);
                updateQuery('sourceIds', sourceIds);
              }}
            />
          </Col>
          <Col flex="1">
            <Select
              name="productIds"
              label="Products"
              isMulti
              options={productOptions}
              value={
                productOptions.filter((mt) =>
                  values.settings.productIds?.some((p: number) => p === mt.value),
                ) ?? []
              }
              onChange={(newValue: any) => {
                const productIds = newValue.map((v: OptionItem) => v.value);
                updateQuery('productIds', productIds);
              }}
            />
          </Col>
        </Row>
        <Row>
          <Col flex="1">
            <Select
              name="seriesIds"
              label="Programs/Shows"
              isMulti
              options={seriesOptions}
              value={
                seriesOptions.filter((mt) =>
                  values.settings.seriesIds?.some((p: number) => p === mt.value),
                ) ?? []
              }
              onChange={(newValue: any) => {
                const seriesIds = newValue.map((v: OptionItem) => v.value);
                updateQuery('seriesIds', seriesIds);
              }}
            />
          </Col>
          <Col flex="1">
            <Select
              name="contributorIds"
              label="Columnists/Anchors"
              isMulti
              options={contributorOptions}
              value={
                contributorOptions.filter((mt) =>
                  values.settings.contributorIds?.some((p: number) => p === mt.value),
                ) ?? []
              }
              onChange={(newValue: any) => {
                const contributorIds = newValue.map((v: OptionItem) => v.value);
                updateQuery('contributorIds', contributorIds);
              }}
            />
          </Col>
        </Row>
        <Row>
          <Col flex="1">
            <Select
              name="actions"
              label="Actions"
              isMulti
              options={actionOptions}
              value={
                actionOptions.filter((mt) =>
                  values.settings.actions?.some(
                    (p: { id: number; valueType: string; value: string }) => p.id === mt.value,
                  ),
                ) ?? []
              }
              onChange={(newValue: any) => {
                const actions = newValue.map((o: OptionItem) => ({
                  id: o.value,
                  valueType: o.label === 'Commentary' ? 'String' : 'Boolean',
                  value: o.label === 'Commentary' ? '*' : 'true',
                }));
                updateQuery('actions', actions);
              }}
            />
          </Col>
          <Col flex="1">
            <SentimentSlider
              label="Sentiment"
              value={values.settings.sentiment ?? []}
              onChange={(value) => {
                updateQuery('sentiment', value);
              }}
            />
          </Col>
        </Row>
      </Col>
      <Col className="code frm-in">
        <label htmlFor="txa-filter">Elasticsearch Query</label>
        <p>
          The query is the expression that is sent to Elasticsearch to find content. Read up on how
          to create a query on the official page{' '}
          <a
            href="https://www.elastic.co/guide/en/elasticsearch/reference/current/search-your-data.html"
            target="_blank"
            rel="noreferrer"
          >
            here
          </a>
          .
        </p>
        <Col className="editor">
          <Editor
            id="txa-filter"
            value={filter}
            onValueChange={(code) => {
              setFilter(code);
              try {
                setFieldValue('query', JSON.parse(code));
              } catch {
                // Ignore errors.
                // TODO: Inform user of formatting issues on blur/validation.
              }
            }}
            highlight={(code) => {
              return highlight(code, languages.json, 'json');
            }}
          />
        </Col>
      </Col>
    </>
  );
};
