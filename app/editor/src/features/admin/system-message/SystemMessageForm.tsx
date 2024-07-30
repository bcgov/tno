import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
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
  HubEventsName,
  IconButton,
  ISystemMessageModel,
  MessageTargetName,
  Modal,
  Row,
  Show,
  useApiAdminHub,
  useModal,
} from 'tno-core';

import { defaultSystemMessage } from './constants';
import * as styled from './styled';

/** The page used to view and edit tags in the administrative section. */
const SystemMessageForm: React.FC = () => {
  const { id } = useParams();
  const { sendMessage } = useApiAdminHub();
  const [, { findSystemMessage, addSystemMessage, updateSystemMessage, deleteSystemMessage }] =
    useSystemMessages();
  const navigate = useNavigate();
  const [systemMessage, setSystemMessage] =
    React.useState<ISystemMessageModel>(defaultSystemMessage);
  const { toggle: toggleConfirmDelete, isShowing: showConfirmDelete } = useModal();
  const { toggle: toggleConfirmMessage, isShowing: showConfirmMessage } = useModal();

  React.useEffect(() => {
    if (id && +id > 0) {
      findSystemMessage(+id)
        .then((data) => {
          if (!!data) setSystemMessage(data);
        })
        .catch(() => {});
    }
    // only want to run on mount
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  const handleSubmit = React.useCallback(
    async (values: ISystemMessageModel) => {
      try {
        const originalId = values.id;
        const result =
          values?.id === 0 ? await addSystemMessage(values) : await updateSystemMessage(values);
        setSystemMessage(result);
        toast.success(`${result.name} has successfully been saved.`);
        if (originalId !== values.id) navigate(`/admin/tags/${result.id}`);
      } catch {}
    },
    [addSystemMessage, navigate, updateSystemMessage],
  );

  return (
    <styled.SystemMessageForm>
      <IconButton
        iconType="back"
        label="Back to Messages"
        className="back-button"
        onClick={() => navigate('/admin/system-messages')}
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
              <Row gap="1rem" alignItems="center">
                <FormikText name="name" label="Name" width={FieldSize.Medium} required />
                <FormikCheckbox label="Is Enabled" name="isEnabled" />
              </Row>
              <FormikTextArea name="description" label="Description" />
              <FormikWysiwyg name="message" label="Message" required />
              <FormikText
                width={FieldSize.Tiny}
                name="sortOrder"
                label="Sort Order"
                type="number"
                className="sort-order"
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
                <Button
                  onClick={toggleConfirmDelete}
                  variant={ButtonVariant.danger}
                  disabled={isSubmitting}
                >
                  Delete
                </Button>
              </Show>
              <Button
                variant={ButtonVariant.secondary}
                disabled={isSubmitting}
                onClick={async () => {
                  toggleConfirmMessage();
                }}
              >
                Send message to all users
              </Button>
            </Row>
            <Modal
              headerText="Confirm Removal"
              body="Are you sure you wish to remove this system message?"
              isShowing={showConfirmDelete}
              hide={toggleConfirmDelete}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await deleteSystemMessage(systemMessage);
                  toast.success(`${systemMessage.name} has successfully been deleted.`);
                  navigate('/admin/tags');
                } finally {
                  toggleConfirmDelete();
                }
              }}
            />
            <Modal
              headerText="Confirm Send Message"
              body="Are you sure you wish to send this message to all users currently logged in?"
              isShowing={showConfirmMessage}
              hide={toggleConfirmMessage}
              type="default"
              confirmText="Yes, Send It"
              onConfirm={async () => {
                try {
                  await sendMessage(HubEventsName.SendAll, MessageTargetName.SystemMessage, values);
                } finally {
                  toggleConfirmMessage();
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
