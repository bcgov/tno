import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { useContent } from 'store/hooks';
import { Button, ButtonVariant, Col, IContentModel, IFilterModel, Row } from 'tno-core';

/**
 * The page used to view and edit report filter.
 * @returns Component.
 */
export const FilterFormPreview: React.FC = () => {
  const { values } = useFormikContext<IFilterModel>();
  const [, { findContentWithElasticsearch }] = useContent();

  const [results, setResults] = React.useState<any>([]);

  const fetchResults = React.useCallback(
    async (filter: IFilterModel) => {
      try {
        const res = await findContentWithElasticsearch(
          filter.query,
          filter.settings.searchUnpublished,
        );
        setResults(res);
      } catch {}
    },
    [findContentWithElasticsearch],
  );

  return (
    <Col>
      <h2>{values.name}</h2>
      <Row>
        <Button variant={ButtonVariant.success} onClick={() => fetchResults(values)}>
          Fetch Results
        </Button>
      </Row>
      <Col className="results" nowrap>
        {results.hits?.hits?.map((h: { _source: IContentModel }, index: number) => {
          const content = h._source;
          return (
            <Row key={content.id} gap="1rem">
              <Col>{index + 1}</Col>
              <Col>{content.source?.code ?? content.otherSource}</Col>
              <Col flex="2">{content.headline}</Col>
            </Row>
          );
        })}
      </Col>
      <hr />
      <Col className="code frm-in">
        <label htmlFor="txa-filter">Elasticsearch Results</label>
        <Col className="editor">
          <Editor
            id="txa-results"
            value={JSON.stringify(results, null, 2)}
            onValueChange={() => {}}
            highlight={(code) => {
              return highlight(code, languages.json, 'json');
            }}
          />
        </Col>
      </Col>
    </Col>
  );
};
