import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { useReportTemplates } from 'store/hooks/admin';
import { useAdminStore } from 'store/slices';
import {
  Button,
  ButtonVariant,
  Col,
  FormikSelect,
  getSortableOptions,
  IOptionItem,
  IReportModel,
  OptionItem,
  Row,
} from 'tno-core';

import { defaultRazorTemplate, defaultReportTemplate } from './constants';

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
export const ReportFormTemplate: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();
  const [{ reportTemplates }] = useAdminStore();
  const [, { findAllReportTemplates }] = useReportTemplates();

  const [templateOptions, setTemplateOptions] = React.useState<IOptionItem[]>(
    getSortableOptions(reportTemplates, [new OptionItem('[New Template]', 0)]),
  );

  React.useEffect(() => {
    findAllReportTemplates()
      .then((templates) =>
        setTemplateOptions(getSortableOptions(templates, [new OptionItem('[New Template]', 0)])),
      )
      .catch(() => {
        // Handled already.
      });
    // Fetch users on initial load only.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    setTemplateOptions(getSortableOptions(reportTemplates, [new OptionItem('[New Template]', 0)]));
  }, [reportTemplates]);

  return (
    <>
      <h2>{values.name}</h2>
      <p>Select a template to build this report with.</p>
      <Row gap="1rem">
        <Col flex="1">
          <FormikSelect
            name="templateId"
            label="Template"
            tooltip="Select a template to base this report on"
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
      <Col className="code frm-in">
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
        <Col className="editor">
          <Editor
            id="txa-subject-template"
            required
            value={values.template.subject}
            onValueChange={(code) => setFieldValue('template.subject', code)}
            highlight={(code) => {
              return highlight(code, languages.cshtml, 'razor');
            }}
          />
        </Col>
      </Col>
      <Col className="code frm-in">
        <label htmlFor="txa-template">Report Template</label>
        <Col className="editor">
          <Editor
            id="txa-body-template"
            required
            value={values.template.body}
            onValueChange={(code) => setFieldValue('template.body', code)}
            highlight={(code) => {
              return highlight(code, languages.cshtml, 'razor');
            }}
          />
        </Col>
      </Col>
    </>
  );
};
