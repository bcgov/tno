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
  IFilterModel,
  IFilterSettingsModel,
  OptionItem,
  Row,
  Select,
  Text,
} from 'tno-core';

/**
 * Generates an Elasticsearch query based on specified 'query'.
 * @param values Form values that will be used to configure the elasticsearch query.
 * @param original Original query object.
 * @returns Elasticsearch query JSON.
 */
const generateQuery = (values: IFilterSettingsModel, original: any = {}) => {
  if (values.size) _.set(original, 'size', values.size);
  if (values.productIds && values.productIds.length)
    _.set(original, 'query.terms.productIds', values.productIds);
  else _.set(original, 'query.terms.productIds', undefined);

  return original;
};

/**
 * The page used to view and edit report filter.
 * @returns Component.
 */
export const FilterFormQuery: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IFilterModel>();
  const [{ productOptions }] = useLookupOptions();

  const [filter, setFilter] = React.useState(JSON.stringify(values.query, null, 2));

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
              setFieldValue('query', {});
              setFilter('');
            }}
          >
            Clear Query
          </Button>
        </Row>
        <Row alignItems="center">
          <Text
            name="query"
            label="Number of Stories"
            type="number"
            width="10ch"
            value={values.query.size ?? 10}
            onChange={(e) => {
              const query = generateQuery(
                { size: !!e.target.value ? parseInt(e.target.value) : 10 },
                values.query,
              );
              setFieldValue('query', query);
              setFilter(JSON.stringify(query, null, 2));
            }}
          />
          <p>
            All filters must have a upward limit of content returned in a single request. The
            default limit is 10.
          </p>
        </Row>
        <Select
          name="query.productIds"
          label="Products"
          isMulti
          options={productOptions}
          value={
            productOptions.filter((mt) =>
              values.query?.query?.terms?.productIds?.some((p: number) => p === mt.value),
            ) ?? []
          }
          onChange={(newValue: any) => {
            const query = generateQuery(
              {
                productIds: newValue.map((v: OptionItem) => v.value),
              },
              values.query,
            );
            setFieldValue('query', query);
            setFilter(JSON.stringify(query, null, 2));
          }}
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
