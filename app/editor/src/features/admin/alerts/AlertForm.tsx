import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useAlerts } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikText,
  FormikTextArea,
  IAlertModel,
  IconButton,
  ITagModel,
  LabelPosition,
  Modal,
  Row,
  Show,
  useModal,
} from 'tno-core';

import { defaultAlert } from './constants';
import * as styled from './styled';

/** The page used to view and edit tags in the administrative section. */
export const AlertForm: React.FC = () => {
  const [, api] = useAlerts();
  const navigate = useNavigate();
  const [alert, setAlert] = React.useState<IAlertModel>(defaultAlert);
  const { toggle, isShowing } = useModal();

  const alertId = !!alert.id ? alert.id : 0;

  React.useEffect(() => {
    api.findAllAlerts().then((data) => {
      if (data.length > 0) setAlert(data[0]);
    });
  }, []);

  const handleSubmit = async (values: IAlertModel) => {
    try {
      const originalId = values.id;
      console.log(values);
      const result = alertId === 0 ? await api.addAlert(values) : await api.updateAlert(values);
      setAlert(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (originalId !== values.id) navigate(`/admin/tags/${result.id}`);
    } catch {}
  };

  return (
    <styled.AlertForm>
      <IconButton
        iconType="back"
        label="Back to Home"
        className="back-button"
        onClick={() => navigate('/admin/tags')}
      />
      <FormikForm
        initialValues={alert}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values, errors }) => (
          <div className="form-container">
            <Col className="form-inputs">
              <FormikText name="name" label="Alert Name" width={FieldSize.Medium} />
              <FormikTextArea
                name="description"
                disabled
                label="Description"
                width={FieldSize.Large}
                value={defaultAlert.description}
              />
              <FormikTextArea
                width={FieldSize.Large}
                name="message"
                label="Message"
                type="text"
                className="message"
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
              body="Are you sure you wish to remove this alert?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteAlert(alert);
                  toast.success(`${alert.name} has successfully been deleted.`);
                  navigate('/admin/tags');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.AlertForm>
  );
};
