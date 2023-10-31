import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { useReports, useReportTemplates } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Checkbox,
  Col,
  FormikSelect,
  IOptionItem,
  IReportModel,
  Overlay,
  Row,
  Show,
} from 'tno-core';

import { defaultRazorTemplate, defaultReportTemplate } from './constants';
import { getReportTemplateOptions } from './utils';

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
export const ReportFormTemplate: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();
  const [{ reportTemplates }, { findAllReportTemplates }] = useReportTemplates();
  const [, { primeReportCache }] = useReports();

  const [enableEdit, setEnableEdit] = React.useState(false);
  const [templateOptions, setTemplateOptions] = React.useState<IOptionItem[]>(
    getReportTemplateOptions(reportTemplates, values.templateId),
  );

  React.useEffect(() => {
    findAllReportTemplates()
      .then((templates) =>
        setTemplateOptions(getReportTemplateOptions(templates, values.templateId)),
      )
      .catch(() => {
        // Handled already.
      });
    // Fetch users on initial load only.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    setTemplateOptions(getReportTemplateOptions(reportTemplates, values.templateId));
  }, [reportTemplates, values.templateId]);

  return (
    <>
      <h2>{values.name}</h2>
      <p>Select a template to build this report with.</p>
      <Row gap="1rem">
        <Col flex="1">
          <FormikSelect
            name="templateId"
            label="Template"
            tooltip="A template is used to generate the report output."
            options={templateOptions}
            value={
              templateOptions.filter(
                (rt) => values.templateId === (rt.value === undefined ? 0 : +rt.value),
              ) ?? ''
            }
            isClearable={false}
            onChange={(newValue) => {
              const option = newValue as IOptionItem;
              const templateId = option.value !== undefined ? +option.value : 0;
              if (templateId) {
                const template = reportTemplates.find((rt) => rt.id === templateId);
                if (template) {
                  setFieldValue('templateId', template.id);
                  setFieldValue('template', template);
                  // trigger caching of a compiled template
                  primeReportCache(values);
                }
              } else {
                setFieldValue('templateId', defaultReportTemplate.id);
                setFieldValue('template', {
                  ...defaultReportTemplate,
                  name: `${values.name}-${Date.now().toString()}`,
                });
              }
            }}
          />
        </Col>
      </Row>
      <hr />
      <Checkbox
        name="enableEdit"
        label="Enable editing template"
        checked={enableEdit}
        onChange={(e) => setEnableEdit(e.target.checked)}
      />
      <Col className="code">
        <Show visible={!enableEdit}>
          <Overlay />
        </Show>
        <Col className="frm-in">
          <Row>
            <Col flex="1">
              <p>Editing this template will change all reports that use this template.</p>
            </Col>
            <Button
              variant={ButtonVariant.secondary}
              onClick={() => setFieldValue('template.body', defaultRazorTemplate)}
            >
              Use Default Template
            </Button>
          </Row>
          <label htmlFor="txa-subject">Subject Template</label>
          <Col className="editor subject">
            <Editor
              id="txa-subject-template"
              value={values.template?.subject ?? ''}
              onValueChange={(code) => setFieldValue('template.subject', code)}
              highlight={(code) => {
                return highlight(code, languages.cshtml, 'razor');
              }}
            />
          </Col>
        </Col>
        <Col className="frm-in">
          <label htmlFor="txa-template">Body Template</label>
          <Col className="editor body">
            <Editor
              id="txa-body-template"
              value={values.template?.body ?? ''}
              onValueChange={(code) => setFieldValue('template.body', code)}
              highlight={(code) => {
                return highlight(code, languages.cshtml, 'razor');
              }}
            />
          </Col>
        </Col>
      </Col>
    </>
  );
};
