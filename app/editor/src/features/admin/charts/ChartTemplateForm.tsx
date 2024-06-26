import { FormikForm } from 'components/formik';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useChartTemplates } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  IChartTemplateModel,
  IconButton,
  Modal,
  Row,
  Show,
  Tab,
  Tabs,
  useModal,
} from 'tno-core';

import { ChartTemplateContextProvider } from './ChartTemplateContext';
import { ChartTemplateFormConfig } from './ChartTemplateFormConfig';
import { ChartTemplateFormDetails } from './ChartTemplateFormDetails';
import { ChartTemplateFormPreview } from './ChartTemplateFormPreview';
import { ChartTemplateFormTemplate } from './ChartTemplateFormTemplate';
import { defaultChartRequestForm, defaultChartTemplate } from './constants';
import * as styled from './styled';

/**
 * The page used to view and edit a chart template.
 * @returns Component.
 */
const ChartTemplateForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const [, { addChartTemplate, deleteChartTemplate, getChartTemplate, updateChartTemplate }] =
    useChartTemplates();
  const { toggle, isShowing } = useModal();

  const [active, setActive] = React.useState('chart');
  const [chartTemplate, setChartTemplate] = React.useState<IChartTemplateModel>({
    ...defaultChartTemplate,
  });

  const chartTemplateId = Number(id);

  React.useEffect(() => {
    if (!!chartTemplateId && chartTemplate?.id !== chartTemplateId) {
      setChartTemplate({ ...defaultChartTemplate, id: chartTemplateId }); // Do this to stop double fetch.
      getChartTemplate(chartTemplateId).then((data) => {
        setChartTemplate(data);
      });
    }
  }, [getChartTemplate, chartTemplate?.id, chartTemplateId]);

  const handleSubmit = async (values: IChartTemplateModel) => {
    try {
      const originalId = values.id;
      const result = !chartTemplate.id
        ? await addChartTemplate(values)
        : await updateChartTemplate(values);
      setChartTemplate(result);

      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/chart/templates/${result.id}`);
    } catch {}
  };

  return (
    <styled.ChartForm>
      <IconButton
        iconType="back"
        label="Back to chart templates"
        className="back-button"
        onClick={() => navigate('/admin/chart/templates')}
      />
      <Row alignSelf="flex-start">
        <p>
          A chart template uses Razor syntax to dynamically generate JSON from a collection of
          content. This JSON data is then sent to the Charts API which generates a graph. Use the{' '}
          <a target="_blank" href="https://www.chartjs.org/docs" rel="noreferrer">
            Charts.js
          </a>{' '}
          data model for the JSON output.
        </p>
      </Row>
      <FormikForm
        initialValues={chartTemplate}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values }) => (
          <ChartTemplateContextProvider
            value={{
              ...defaultChartRequestForm,
              template: values.template,
              settings: values.sectionSettings ?? defaultChartRequestForm.settings,
            }}
          >
            <Tabs
              tabs={
                <>
                  <Tab
                    label="Details"
                    onClick={() => {
                      setActive('chart');
                    }}
                    active={active === 'chart'}
                  />
                  <Tab
                    label="Template"
                    onClick={() => {
                      setActive('template');
                    }}
                    active={active === 'template'}
                  />
                  <Tab
                    label="Chart.JS Configuration"
                    onClick={() => {
                      setActive('config');
                    }}
                    active={active === 'config'}
                  />
                  <Tab
                    label="Preview"
                    onClick={() => {
                      setActive('preview');
                    }}
                    active={active === 'preview'}
                  />
                </>
              }
            >
              <div className="form-container">
                <Show visible={active === 'chart'}>
                  <ChartTemplateFormDetails />
                </Show>
                <Show visible={active === 'template'}>
                  <ChartTemplateFormTemplate />
                </Show>
                <Show visible={active === 'config'}>
                  <ChartTemplateFormConfig />
                </Show>
                <Show visible={active === 'preview'}>
                  <ChartTemplateFormPreview />
                </Show>
                <Row justifyContent="center" className="form-inputs">
                  <Button type="submit" disabled={isSubmitting}>
                    Save
                  </Button>
                  <Show visible={!!values.id}>
                    <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                      Delete
                    </Button>
                  </Show>
                </Row>
                <Modal
                  headerText="Confirm Removal"
                  body="Are you sure you wish to remove this chart template?"
                  isShowing={isShowing}
                  hide={toggle}
                  type="delete"
                  confirmText="Yes, Remove It"
                  onConfirm={async () => {
                    try {
                      await deleteChartTemplate(chartTemplate);
                      toast.success(`${chartTemplate.name} has successfully been deleted.`);
                      navigate('/admin/chart/templates');
                    } catch {
                      // Globally handled
                    } finally {
                      toggle();
                    }
                  }}
                />
              </div>
            </Tabs>
          </ChartTemplateContextProvider>
        )}
      </FormikForm>
    </styled.ChartForm>
  );
};

export default ChartTemplateForm;
