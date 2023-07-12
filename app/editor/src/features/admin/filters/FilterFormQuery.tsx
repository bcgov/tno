import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { useFormikContext } from 'formik';
import _ from 'lodash';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { useLookupOptions } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  FormikSelect,
  FormikText,
  IFilterModel,
  IFilterSettingsModel,
  OptionItem,
  Row,
} from 'tno-core';

/**
 * Generates an Elasticsearch query based on specified 'settings'.
 * @param settings Form values that will be used to configure the elasticsearch query.
 * @returns Elasticsearch query JSON.
 */
const generateQuery = (settings: IFilterSettingsModel) => {
  let query: any = {};
  if (settings.size) _.set(query, 'size', settings.size);
  if (settings.productIds) _.set(query, 'query.terms.productIds', settings.productIds);

  return query;
};

/**
 * The page used to view and edit report filter.
 * @returns Component.
 */
export const FilterFormQuery: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IFilterModel>();
  const [{ productOptions }] = useLookupOptions();

  const [filter, setFilter] = React.useState(JSON.stringify(values.query, null, 2));

  React.useEffect(() => {
    console.debug(values.settings);
    var query = generateQuery(values.settings);
    setFilter(JSON.stringify(query, null, 2));
    setFieldValue('query', query);
  }, [setFieldValue, values.settings]);

  return (
    <>
      <Col>
        <h2>{values.name}</h2>
        <Row>
          <Col flex="1">
            <p>
              A primary filter can be used to find content to include in the report. If a report has
              sections, you can add filters to each section instead.
            </p>
          </Col>
          <Button
            variant={ButtonVariant.secondary}
            onClick={() => {
              setFieldValue('settings', {});
            }}
          >
            Clear Filter
          </Button>
        </Row>
        <Row alignItems="center">
          <FormikText
            name="settings.size"
            label="Number of Stories"
            type="number"
            width="10ch"
            onChange={(e) => {
              setFieldValue(
                'settings.size',
                !!e.target.value ? parseInt(e.target.value) : undefined,
              );
            }}
          />
          <p>
            All filters must have a upward limit of content returned in a single request. The
            default limit is 10.
          </p>
        </Row>
        <FormikSelect
          name="productIds"
          label="Products"
          isMulti
          value={
            productOptions.filter((mt) =>
              values.settings.productIds?.some((p: number) => p === mt.value),
            ) ?? []
          }
          onChange={(newValue: any) => {
            console.debug(newValue);
            setFieldValue(
              'settings.productIds',
              newValue.length ? newValue.map((v: OptionItem) => v.value) : undefined,
            );
          }}
          options={productOptions}
        />
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
                const json = JSON.parse(code);
                setFieldValue('filter', json);
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
