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
  FieldSize,
  FormikSelect,
  FormikText,
  IReportModel,
  Row,
} from 'tno-core';

/**
 * The page used to view and edit report filter.
 * @returns Component.
 */
export const ReportFormFilter: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();
  const [{ productOptions }] = useLookupOptions();

  const [filter, setFilter] = React.useState(JSON.stringify(values.filter, null, 2));

  return (
    <>
      <Col>
        <h2>{values.name}</h2>
        <p>
          A primary filter can be used to find content to include in the report. If a report has
          sections, you can add filters to each section instead.
        </p>
        <div>
          <Button
            variant={ButtonVariant.secondary}
            onClick={() => {
              setFieldValue('filter', {});
              setFilter('{}');
            }}
          >
            Clear Filter
          </Button>
        </div>
        <Row>
          <FormikText
            name="filter.size"
            label="Number of Stories"
            type="number"
            width="10ch"
            onChange={(e) => {
              if (!!e.target.value) {
                const value = parseInt(e.target.value);
                setFieldValue('filter.size', value);
                setFilter(JSON.stringify({ ...values.filter, size: value }, null, 2));
              }
            }}
          />
          <p>
            All filters must have a upward limit of content returned in a single request. The
            default limit is 10.
          </p>
        </Row>
        <FormikSelect
          name="productId"
          value={
            productOptions.find((mt) => mt.value === values.filter?.query?.match?.productId) ?? ''
          }
          onChange={(newValue: any) => {
            if (!!newValue) {
              const filter = {
                ...values.filter,
                query: {
                  ...values.filter?.query,
                  match: { ...values.filter?.query?.match, productId: newValue.value },
                },
              };
              setFieldValue('filter', filter);
              setFilter(JSON.stringify(filter, null, 2));
            } else {
              setFieldValue('filter.query', {});
              setFilter(JSON.stringify({ ...values.filter, query: {} }, null, 2));
            }
          }}
          label="Product"
          width={FieldSize.Small}
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
