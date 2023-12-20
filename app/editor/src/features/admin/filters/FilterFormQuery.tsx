import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { Button, ButtonVariant, Col, IFilterModel, Row } from 'tno-core';

import { FilterSettingsForm } from './FilterSettingsForm';
import { useElastic } from './hooks';

/**
 * The page used to view and edit report filter.
 * @returns Component.
 */
export const FilterFormQuery: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IFilterModel>();
  const generateQuery = useElastic();

  const [filter, setFilter] = React.useState(JSON.stringify(values.query, null, 2));

  return (
    <>
      <Col gap="0.5rem">
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
        <FilterSettingsForm
          path="settings"
          onChange={(settings) => {
            const query = generateQuery(settings, values.query);
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
