import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useTopics } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getEnumStringOptions,
  IconButton,
  ITopicModel,
  LabelPosition,
  Modal,
  Row,
  Show,
  TopicTypeName,
  useModal,
} from 'tno-core';

import { defaultTopic } from './constants';
import * as styled from './styled';
import { TopicSchema } from './validation';

/** The page used to view and edit tags in the administrative section. */
export const TopicForm: React.FC = () => {
  const [, api] = useTopics();
  const { state } = useLocation();
  const { id } = useParams();
  const navigate = useNavigate();
  const { toggle, isShowing } = useModal();

  const [topic, setTopic] = React.useState<ITopicModel>((state as any)?.topic ?? defaultTopic);

  const topicId = Number(id);
  const topicTypeOptions = getEnumStringOptions(TopicTypeName);

  React.useEffect(() => {
    if (!!topicId && topic?.id !== topicId) {
      setTopic({ ...defaultTopic, id: topicId }); // Do this to stop double fetch.
      api.getTopic(topicId).then((data) => {
        setTopic(data);
      });
    }
  }, [api, topic?.id, topicId]);

  const handleSubmit = async (values: ITopicModel) => {
    try {
      const originalId = values.id;
      const result = !topic.id ? await api.addTopic(values) : await api.updateTopic(values);
      setTopic(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/topics/${result.id}`);
    } catch {}
  };

  return (
    <styled.TopicForm>
      <IconButton
        iconType="back"
        label="Back to Topics"
        className="back-button"
        onClick={() => navigate('/admin/topics')}
      />
      <FormikForm
        initialValues={topic}
        validationSchema={TopicSchema}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values }) => (
          <div className="form-container">
            <Col className="form-inputs">
              <FormikText width={FieldSize.Large} name="name" label="Name" />
              <FormikTextArea name="description" label="Description" width={FieldSize.Large} />
              <FormikSelect
                label="Topic Type"
                name="topicType"
                options={topicTypeOptions}
                value={topicTypeOptions.find((o) => o.value === values.topicType)}
                width={FieldSize.Medium}
              />
              <FormikText
                width={FieldSize.Tiny}
                name="sortOrder"
                label="Sort Order"
                type="number"
                className="sort-order"
              />
              <FormikCheckbox
                labelPosition={LabelPosition.Top}
                label="Auto Transcribe"
                name="autoTranscribe"
              />
              <FormikCheckbox
                labelPosition={LabelPosition.Top}
                label="Is Enabled"
                name="isEnabled"
              />
              <Show visible={!!values.id}>
                <Row>
                  <FormikText
                    width={FieldSize.Small}
                    disabled
                    name="updatedBy"
                    label="Updated By"
                  />
                  <FormikDatePicker
                    selectedDate={
                      !!values.updatedOn ? moment(values.updatedOn).toString() : undefined
                    }
                    onChange={noop}
                    name="updatedOn"
                    label="Updated On"
                    disabled
                    width={FieldSize.Small}
                  />
                </Row>
                <Row>
                  <FormikText
                    width={FieldSize.Small}
                    disabled
                    name="createdBy"
                    label="Created By"
                  />
                  <FormikDatePicker
                    selectedDate={
                      !!values.createdOn ? moment(values.createdOn).toString() : undefined
                    }
                    onChange={noop}
                    name="createdOn"
                    label="Created On"
                    disabled
                    width={FieldSize.Small}
                  />
                </Row>
              </Show>
            </Col>
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
              body="Are you sure you wish to remove this topic?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteTopic(topic);
                  toast.success(`${topic.name} has successfully been deleted.`);
                  navigate('/admin/topics');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.TopicForm>
  );
};
