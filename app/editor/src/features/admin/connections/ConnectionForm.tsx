import { IconButton, LabelPosition } from 'components/form';
import {
  FormikCheckbox,
  FormikForm,
  FormikSelect,
  FormikText,
  FormikTextArea,
} from 'components/formik';
import { FormikDatePicker } from 'components/formik/datepicker';
import { Modal } from 'components/modal';
import { IConnectionModel, useModal, useTooltips } from 'hooks';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useConnections } from 'store/hooks/admin';
import { Button, ButtonVariant, Col, FieldSize, Row, Show } from 'tno-core';

import { connectionTypeOptions, defaultConnection } from './constants';
import * as styled from './styled';

/** The page used to view and edit tags in the administrative section. */
export const ConnectionForm: React.FC = () => {
  const [, api] = useConnections();
  const { state } = useLocation();
  const { id } = useParams();
  const navigate = useNavigate();
  const { toggle, isShowing } = useModal();
  useTooltips();

  const connectionId = Number(id);
  const [connection, setConnection] = React.useState<IConnectionModel>(
    (state as any)?.connection ?? defaultConnection,
  );
  const [settings, setSettings] = React.useState('{}');

  React.useEffect(() => {
    if (!!connectionId && connection?.id !== connectionId) {
      api.getConnection(connectionId).then((data) => {
        setConnection(data);
        setSettings(JSON.stringify(data.configuration));
      });
    }
  }, [api, connection?.id, connectionId]);

  const handleSubmit = async (values: IConnectionModel) => {
    try {
      const originalId = values.id;
      const result = !connection.id
        ? await api.addConnection(values)
        : await api.updateConnection(values);
      setConnection(result);
      setSettings(JSON.stringify(result.configuration));
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/connections/${result.id}`);
    } catch {}
  };

  return (
    <styled.ConnectionForm>
      <IconButton
        iconType="back"
        label="Back to Connections"
        className="back-button"
        onClick={() => navigate('/admin/connections')}
      />
      <FormikForm
        initialValues={connection}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values, setFieldValue }) => (
          <div className="form-container">
            <Col className="form-inputs">
              <FormikText width={FieldSize.Large} name="name" label="Name" />
              <FormikTextArea name="description" label="Description" width={FieldSize.Large} />
              <FormikSelect
                name="connectionType"
                label="Type"
                width={FieldSize.Big}
                value={connectionTypeOptions.find((o) => o.value === values.connectionType) ?? ''}
                onChange={(newValue: any) => {
                  setFieldValue('connectionType', newValue.value);
                }}
                options={connectionTypeOptions}
                required
              />
              {/* TODO: Configuration settings form based on the connection type */}
              <FormikTextArea
                name="settings"
                label="Configuration"
                tooltip="Must be valid JSON"
                value={settings}
                onChange={(e) => {
                  setSettings(e.currentTarget.value);
                }}
                onBlur={(e) => {
                  setFieldValue('configuration', JSON.parse(e.currentTarget.value));
                }}
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
              <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                Delete
              </Button>
            </Row>
            <Modal
              headerText="Confirm Removal"
              body="Are you sure you wish to remove this connection?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteConnection(connection);
                  toast.success(`${connection.name} has successfully been deleted.`);
                  navigate('/admin/connections');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.ConnectionForm>
  );
};
