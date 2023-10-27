import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { Button, ButtonVariant, Col, generateQuery, INotificationModel, Row } from 'tno-core';

import { FilterSettingsForm } from '../filters';

export const NotificationFilterForm = () => {
  const { values, setFieldValue } = useFormikContext<INotificationModel>();

  const [filter, setFilter] = React.useState(JSON.stringify(values.query, null, 2));

  React.useEffect(() => {
    setFilter(JSON.stringify(values.query, null, 2));
  }, [values.query]);

  return (
    <Col>
      <Row>
        <Col flex="1">
          <p>
            A filter provides a way to limit which notifications are sent out to subscribers. Each
            time an event triggers this notification it will validate that the content passes this
            filter.
          </p>
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
        supportsElasticQuery={false}
        onChange={(settings) => {
          const query = generateQuery(settings, values.query);
          setFieldValue('query', query);
          setFilter(JSON.stringify(query, null, 2));
        }}
      />
      <Col className="code frm-in">
        <label htmlFor="txa-filter">JSON Filter</label>
        <Col className="editor">
          <Editor
            id="txa-filter"
            value={filter}
            disabled={true}
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
    </Col>
  );
};
