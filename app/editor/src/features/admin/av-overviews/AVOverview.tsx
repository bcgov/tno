import { FormikForm } from 'components/formik';
import React from 'react';
import { FaSpinner } from 'react-icons/fa';
import { MdAdd, MdSave } from 'react-icons/md';
import { useAVOverviewTemplates, useReportTemplates } from 'store/hooks/admin';
import {
  AVOverviewTemplateTypeName,
  Button,
  ButtonVariant,
  FormikSelect,
  Row,
  Select,
  Show,
} from 'tno-core';

import {
  defaultAVOverviewTemplate,
  defaultAVOverviewTemplateSection,
  getReportTemplateOptions,
  templateTypeOptions,
} from './constants';
import { IAVOverviewTemplateForm } from './interfaces';
import { OverviewSection } from './OverviewSection';
import * as styled from './styled';

/** Evening overview section, contains table of items, and list of overview sections */
const AVOverview: React.FC = () => {
  const [api] = useAVOverviewTemplates();
  const [{ reportTemplates }, { findAllReportTemplates }] = useReportTemplates();

  const [templateType, setTemplateType] = React.useState(AVOverviewTemplateTypeName.Weekday);
  const [template, setTemplate] = React.useState<IAVOverviewTemplateForm>(
    defaultAVOverviewTemplate(AVOverviewTemplateTypeName.Weekday, 0),
  );

  const templateOptions = getReportTemplateOptions(reportTemplates);

  React.useEffect(() => {
    findAllReportTemplates();
    // Only fetch on initial load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Fetch the template for the template type.
  React.useEffect(() => {
    api
      .getAVOverview(templateType)
      .then((data) => {
        if (data) setTemplate(data);
        else {
          setTemplate(defaultAVOverviewTemplate(AVOverviewTemplateTypeName.Weekday, 0));
        }
      })
      .catch(() => {});
  }, [api, templateType]);

  const handleSubmit = React.useCallback(
    async (values: IAVOverviewTemplateForm) => {
      try {
        const template = values.isNew
          ? await api.addAVOverview(values)
          : await api.updateAVOverview(values);
        setTemplate(template);
      } catch {}
    },
    [api],
  );

  return (
    <styled.AVOverview>
      <Row className="page-header">
        <div className="title">Evening Overview Template</div>
      </Row>
      <p>
        An Evening Overview Template provides a way to configure the default sections and stories
        within each section.
      </p>

      <FormikForm
        initialValues={template}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values, setFieldValue }) => (
          <>
            <Row className="buttons">
              <Select
                label="Template Type"
                name="templateType"
                options={templateTypeOptions}
                value={templateTypeOptions.find((o) => o.value === templateType) ?? ''}
                onChange={(newValue: any) => {
                  setTemplateType(newValue.value);
                }}
                isClearable={false}
                width="20ch"
              />
              <FormikSelect
                label="Report Template"
                name="templateId"
                options={templateOptions}
                value={templateOptions.find((o) => o.value === values.templateId) ?? ''}
                isClearable={false}
              />
              <Button
                onClick={() => {}}
                variant={ButtonVariant.success}
                type="submit"
                disabled={!values.sections.length}
                className="save-items"
              >
                Save Template
                {isSubmitting ? (
                  <FaSpinner className="icon spinner" />
                ) : (
                  <MdSave className="icon" />
                )}
              </Button>
            </Row>
            {values.sections.map((section, index) => (
              <OverviewSection key={index} index={index} />
            ))}
            <Row className="buttons">
              <Button
                variant={ButtonVariant.action}
                onClick={() =>
                  setFieldValue('sections', [
                    ...values.sections,
                    defaultAVOverviewTemplateSection(values.templateType),
                  ])
                }
              >
                New broadcast section <MdAdd className="icon" />
              </Button>
              <Show visible={!!values.sections.length}>
                <Button
                  onClick={() => {}}
                  variant={ButtonVariant.success}
                  type="submit"
                  className="save-items"
                >
                  Save Template
                  {isSubmitting ? (
                    <FaSpinner className="icon spinner" />
                  ) : (
                    <MdSave className="icon" />
                  )}
                </Button>
              </Show>
            </Row>
          </>
        )}
      </FormikForm>
    </styled.AVOverview>
  );
};

export default AVOverview;
