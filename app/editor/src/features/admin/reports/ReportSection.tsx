import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { Col, FormikText, FormikTextArea, IReportModel, Row } from 'tno-core';

import * as styled from './styled';

export interface IReportSectionProps {
  /** Class name of component. */
  className?: string;
  /** Index position within sections. */
  index: number;
}

export const ReportSection: React.FC<IReportSectionProps> = ({ className, index }) => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();

  const [filter, setFilter] = React.useState(
    JSON.stringify(values.settings.sections[index].filter ?? {}, null, 2),
  );

  React.useEffect(() => {
    setFilter(JSON.stringify(values.settings.sections[index].filter ?? {}, null, 2));
  }, [index, values.settings.sections]);

  return (
    <styled.ReportSection className={className}>
      <Row>
        <FormikText
          name={`settings.sections.${index}.name`}
          label="Name"
          tooltip="Unique name to identify the section"
          placeholder="Enter unique name"
          required
          maxLength={100}
        />
        <FormikText
          name={`settings.sections.${index}.label`}
          label="Label"
          tooltip="A label to display at the beginning of the section"
          maxLength={100}
        />
      </Row>
      <FormikTextArea
        name={`settings.sections.${index}.description`}
        label="Description"
        placeholder="Describe the purpose of this section"
      />
      <Col className="code frm-in">
        <label htmlFor="txa-section-template">Elasticsearch Query</label>
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
            id="txa-section-template"
            value={filter}
            onValueChange={(code) => {
              setFilter(code);
              try {
                const json = JSON.parse(code);
                setFieldValue(`settings.sections.${index}.filter`, json);
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
    </styled.ReportSection>
  );
};
