import { FormikForm } from 'components/formik';
import moment, { Moment } from 'moment';
import React from 'react';
import { FaBinoculars, FaPaperPlane, FaSpinner } from 'react-icons/fa';
import { MdAdd, MdSave } from 'react-icons/md';
import { toast } from 'react-toastify';
import { useAVOverviewInstances } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  IAVOverviewInstanceModel,
  Row,
  SelectDate,
  Show,
} from 'tno-core';

import {
  defaultAVOverviewInstance,
  defaultAVOverviewSection,
  defaultAVOverviewTemplate,
} from './constants';
import { OverviewSection } from './OverviewSection';
import * as styled from './styled';
import { getIsEditable } from './utils';

/** Evening overview section, contains table of items, and list of overview sections */
export const AVOverview: React.FC = () => {
  const [api] = useAVOverviewInstances();

  const [publishedOn, setPublishedOn] = React.useState<Moment>(moment(Date.now()));
  const [instance, setInstance] = React.useState<IAVOverviewInstanceModel>(
    defaultAVOverviewInstance(defaultAVOverviewTemplate, publishedOn.toDate()),
  );
  const [isEditable, setIsEditable] = React.useState(getIsEditable(publishedOn));

  React.useEffect(() => {
    setIsEditable(getIsEditable(publishedOn));
    api
      .findAVOverview(publishedOn.local().format('MM/DD/yyyy'))
      .then((data) => {
        if (data) setInstance(data);
        else {
          toast.error('An evening overview could not be generated for the selected date.');
          setInstance(defaultAVOverviewInstance(defaultAVOverviewTemplate, publishedOn.toDate()));
        }
      })
      .catch();
  }, [api, publishedOn]);

  const handleSubmit = React.useCallback(
    async (values: IAVOverviewInstanceModel) => {
      try {
        const instance =
          values.id === 0 ? await api.addAVOverview(values) : await api.updateAVOverview(values);
        setInstance(instance);
      } catch {}
    },
    [api],
  );

  return (
    <styled.AVOverview>
      <Row className="page-header">
        <div className="title">AV Evening Overview</div>
        <div className="buttons">
          <Button disabled={true || !instance.id}>
            Preview <FaBinoculars className="icon" />
          </Button>
          <Button disabled={true || !instance.id}>
            Publish <FaPaperPlane className="icon" />
          </Button>
        </div>
      </Row>

      <FormikForm
        initialValues={instance}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values)
            .catch((err) => toast.warning(() => 'Failed to save.'))
            .finally(() => toast.success(() => 'Evening overview saved.'));
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values, setFieldValue }) => (
          <>
            <Row className="buttons">
              <SelectDate
                name="publishedOn"
                placeholderText="mm/dd/yyyy"
                selected={publishedOn.toDate()}
                width="8em"
                onChange={(date) => {
                  setPublishedOn(moment(date));
                }}
              />
              <Col justifyContent="center">
                <h2>{values.templateType}</h2>
              </Col>
              <Button
                onClick={() => {}}
                variant={ButtonVariant.success}
                type="submit"
                disabled={!values.sections.length || !isEditable}
                className="save-items"
              >
                Save Evening Overview
                {isSubmitting ? (
                  <FaSpinner className="icon spinner" />
                ) : (
                  <MdSave className="icon" />
                )}
              </Button>
            </Row>
            {values.sections.map((section, index) => (
              <OverviewSection key={index} index={index} editable={isEditable} />
            ))}
            <Row className="buttons">
              <Button
                variant={ButtonVariant.action}
                onClick={() =>
                  setFieldValue('sections', [
                    ...values.sections,
                    defaultAVOverviewSection(values.id),
                  ])
                }
                disabled={!isEditable}
              >
                New broadcast section <MdAdd className="icon" />
              </Button>
              <Show visible={!!values.sections.length}>
                <Button
                  onClick={() => {}}
                  variant={ButtonVariant.success}
                  type="submit"
                  className="save-items"
                  disabled={!isEditable}
                >
                  Save Evening Overview
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
