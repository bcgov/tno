import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { toast } from 'react-toastify';
import { useLookup } from 'store/hooks';
import { useReportTemplates } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FormikSelect,
  getEnumStringOptions,
  IOptionItem,
  IReportTemplateModel,
  ReportTypeName,
  Row,
  Settings,
} from 'tno-core';

import {
  defaultAVOverviewBodyRazorTemplate,
  defaultAVOverviewSubjectRazorTemplate,
} from './constants';

/**
 * The page used to view and edit a report template.
 * @returns Component.
 */
export const ReportTemplateFormTemplate: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IReportTemplateModel>();
  const [{ isReady, settings }] = useLookup();
  const [, { getReportTemplate }] = useReportTemplates();

  const [defaultReportTemplateId, setDefaultReportTemplateId] = React.useState(0);
  const [defaultReportTemplate, setDefaultReportTemplate] = React.useState<IReportTemplateModel>();

  React.useEffect(() => {
    if (isReady) {
      const defaultReportTemplateId = settings.find(
        (s) => s.name === Settings.DefaultReportTemplate,
      )?.value;
      if (defaultReportTemplateId) setDefaultReportTemplateId(+defaultReportTemplateId);
      else toast.error(`Configuration settings '${Settings.DefaultReportTemplate}' is required.`);
    }
  }, [isReady, settings]);

  const getTemplate = React.useCallback(async () => {
    if (!defaultReportTemplate) {
      const template = await getReportTemplate(defaultReportTemplateId);
      setDefaultReportTemplate(template);
      return template;
    } else {
      return defaultReportTemplate;
    }
  }, [defaultReportTemplate, defaultReportTemplateId, getReportTemplate]);

  const reportTypeOptions = getEnumStringOptions(ReportTypeName);

  return (
    <>
      <h2>{values.name}</h2>
      <p>Editing this template will change all reports that use this template.</p>
      <Row alignItems="flex-end">
        <FormikSelect
          name="reportType"
          label="Report Type"
          options={reportTypeOptions}
          tooltip="Each report type generates a different kind of report.  Choose the one this template is designed for."
          value={reportTypeOptions.filter((rt) => values.reportType === rt.value) ?? ''}
          isClearable={false}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            setFieldValue('reportType', option.value);
          }}
        />
        <Col flex="1"></Col>
        <Button
          variant={ButtonVariant.secondary}
          onClick={async () => {
            try {
              const template = await getTemplate();
              setFieldValue(
                'subject',
                values.reportType === ReportTypeName.Content
                  ? template.subject
                  : defaultAVOverviewSubjectRazorTemplate,
              );
              setFieldValue(
                'body',
                values.reportType === ReportTypeName.Content
                  ? template.body
                  : defaultAVOverviewBodyRazorTemplate,
              );
            } catch {
              toast.error('Failed to fetch default report template');
            }
          }}
        >
          Use Default Template
        </Button>
      </Row>
      <Col className="code frm-in">
        <label htmlFor="txa-subject" className="required">
          Subject Template
        </label>
        <Col className="editor">
          <Editor
            id="txa-subject-template"
            required
            value={values.subject}
            onValueChange={(code) => setFieldValue('subject', code)}
            highlight={(code) => {
              return highlight(code, languages.cshtml, 'razor');
            }}
          />
        </Col>
      </Col>
      <Col className="code frm-in">
        <label htmlFor="txa-template" className="required">
          Report Template
        </label>
        <Col className="editor">
          <Editor
            id="txa-body-template"
            required
            value={values.body}
            onValueChange={(code) => setFieldValue('body', code)}
            highlight={(code) => {
              return highlight(code, languages.cshtml, 'razor');
            }}
          />
        </Col>
      </Col>
    </>
  );
};
