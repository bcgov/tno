import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useSystemMessages } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikText,
  FormikTextArea,
  FormikWysiwyg,
  IconButton,
  ISystemMessageModel,
  LabelPosition,
  Modal,
  Row,
  Show,
  useModal,
} from 'tno-core';

import { defaultSystemMessage } from './constants';
import * as styled from './styled';

/** The page used to view and edit tags in the administrative section. */
const SystemMessageForm: React.FC = () => {
  const [, api] = useSystemMessages();
  const navigate = useNavigate();
  const [systemMessage, setSystemMessage] =
    React.useState<ISystemMessageModel>(defaultSystemMessage);
  const { toggle, isShowing } = useModal();

  const systemMessageId = !!systemMessage.id ? systemMessage.id : 0;

  React.useEffect(() => {
    api
      .findSystemMessage()
      .then((data) => {
        if (!!data) setSystemMessage(data);
      })
      .catch(() => {});
    // only want to run on mount
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleSubmit = async (values: ISystemMessageModel) => {
    try {
      const originalId = values.id;
      const result =
        systemMessageId === 0
          ? await api.addSystemMessage(values)
          : await api.updateSystemMessage(values);
      setSystemMessage(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (originalId !== values.id) navigate(`/admin/tags/${result.id}`);
    } catch {}
  };

  return (
    <styled.SystemMessageForm>
      <IconButton
        iconType="back"
        label="Back to Home"
        className="back-button"
        onClick={() => navigate('/admin/tags')}
      />
      <FormikForm
        initialValues={systemMessage}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values, errors }) => (
          <div className="form-container">
            <Col className="form-inputs">
              <FormikText name="name" label="System Message Name" width={FieldSize.Medium} />
              <FormikTextArea
                name="description"
                disabled
                label="Description"
                width={FieldSize.Large}
                value={defaultSystemMessage.description}
              />
              <FormikWysiwyg name="message" label="Message" />

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
              body="Are you sure you wish to remove this system message?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteSystemMessage(systemMessage);
                  toast.success(`${systemMessage.name} has successfully been deleted.`);
                  navigate('/admin/tags');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.SystemMessageForm>
  );
};

export default SystemMessageForm;
