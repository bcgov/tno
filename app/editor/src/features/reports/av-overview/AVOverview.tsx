import { FormikForm } from 'components/formik';
import moment, { Moment } from 'moment';
import React from 'react';
import {
  FaArrowAltCircleLeft,
  FaArrowAltCircleRight,
  FaBinoculars,
  FaPaperPlane,
  FaSpinner,
} from 'react-icons/fa';
import { MdAdd, MdSave } from 'react-icons/md';
import { useSearchParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useAVOverviewInstances } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  IAVOverviewInstanceModel,
  Modal,
  Row,
  SelectDate,
  Show,
  useModal,
} from 'tno-core';

import {
  defaultAVOverviewInstance,
  defaultAVOverviewSection,
  defaultAVOverviewTemplate,
} from './constants';
import { OverviewSection } from './OverviewSection';
import * as styled from './styled';
import { generateListOfSummaries, getIsEditable, ISectionSummary } from './utils';

/** Evening overview section, contains table of items, and list of overview sections */
const AVOverview: React.FC = () => {
  const [api] = useAVOverviewInstances();
  const [params] = useSearchParams();
  const { toggle, isShowing } = useModal();
  const queryDate = params.get('date') ? moment(params.get('date')) : moment(Date.now());

  const [publishedOn, setPublishedOn] = React.useState<Moment>(queryDate);
  const [instance, setInstance] = React.useState<IAVOverviewInstanceModel>(
    defaultAVOverviewInstance(defaultAVOverviewTemplate, publishedOn.toDate()),
  );
  const [summaries, setSummaries] = React.useState<ISectionSummary[]>([]);
  // Unlock Evening Overview to allow editing prior days
  const [isEditable] = React.useState(true || !instance.isPublished || getIsEditable(publishedOn));

  React.useEffect(() => {
    // Unlock Evening Overview to allow editing prior days
    // setIsEditable(getIsEditable(publishedOn));
    api
      .findAVOverview(publishedOn.local().format('MM/DD/yyyy'))
      .then((data) => {
        if (data) {
          setInstance(data);
          // Unlock Evening Overview to allow editing prior days
          // if (data.isPublished) setIsEditable(!data.isPublished);
          const date = moment(data.publishedOn).utc();
          window.history.pushState({}, '', `?date=${date.format('yyyy/MM/DD')}`);
        } else {
          toast.error('An evening overview could not be generated for the selected date.');
          setInstance(defaultAVOverviewInstance(defaultAVOverviewTemplate, publishedOn.toDate()));
        }
      })
      .catch(() => {});
  }, [api, instance.isPublished, publishedOn]);

  React.useEffect(() => {
    const summaryList = generateListOfSummaries(instance.sections);
    setSummaries(summaryList);
  }, [instance]);

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

  const handlePublish = React.useCallback(async () => {
    try {
      const result = await api.publishAVOverview(instance.id);
      toast.success('Evening Overview report request to publish has been sent.');
      setInstance(result);
    } catch {}
  }, [api, instance.id]);

  return (
    <styled.AVOverview>
      <Row className="page-header">
        <h1>Evening Overview</h1>
        <div className="buttons">
          <Button
            disabled={!instance.id}
            onClick={() => window.open(`evening-overview/${instance.id}`, '_blank')}
          >
            Preview <FaBinoculars className="icon" />
          </Button>
          <Button disabled={!instance.id} onClick={() => toggle()}>
            Publish <FaPaperPlane className="icon" />
          </Button>
        </div>
      </Row>

      <FormikForm
        initialValues={instance}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values, setFieldValue }) => (
          <>
            <Row className="buttons">
              <Row className="date-filter">
                <Button
                  variant={ButtonVariant.secondary}
                  onClick={() => {
                    setPublishedOn((value) => {
                      const result = value.add(-1, 'd').clone();
                      window.history.pushState({}, '', `?date=${result.format('yyyy/MM/DD')}`);
                      return result;
                    });
                  }}
                >
                  <FaArrowAltCircleLeft />
                </Button>
                <SelectDate
                  name="publishedOn"
                  placeholderText="mm/dd/yyyy"
                  selected={publishedOn.toDate()}
                  onChange={(date) => {
                    const value = moment(date);
                    setPublishedOn(value);
                    window.history.pushState({}, '', `?date=${value.format('yyyy/MM/DD')}`);
                  }}
                />
                <Button
                  variant={ButtonVariant.secondary}
                  onClick={() => {
                    setPublishedOn((value) => {
                      const result = value.add(1, 'd').clone();
                      window.history.pushState({}, '', `?date=${result.format('yyyy/MM/DD')}`);
                      return result;
                    });
                  }}
                >
                  <FaArrowAltCircleRight />
                </Button>
              </Row>
              <Col justifyContent="center">
                <h2>{values.templateType}</h2>
              </Col>
              <Button
                onClick={() => {}}
                variant={ButtonVariant.success}
                type="submit"
                disabled={!values.sections?.length || !isEditable}
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
            {values.sections?.map((section, index) => (
              <OverviewSection
                key={index}
                index={index}
                editable={isEditable}
                summaries={summaries}
                setSummaries={setSummaries}
              />
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
              <Show visible={!!values.sections?.length}>
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
            <Modal
              headerText="Confirm Publish"
              body={
                values.isPublished
                  ? 'This evening overview has already been published. Are you sure you wish to publish again?'
                  : 'Are you sure you wish to publish the evening overview?'
              }
              isShowing={isShowing}
              hide={toggle}
              type="default"
              confirmText="Yes, Publish"
              onConfirm={async () => {
                try {
                  await handlePublish();
                } catch {
                  // Globally handled
                } finally {
                  toggle();
                }
              }}
            />
          </>
        )}
      </FormikForm>
    </styled.AVOverview>
  );
};

export default AVOverview;
