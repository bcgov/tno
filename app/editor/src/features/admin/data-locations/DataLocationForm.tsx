import { FormikForm } from 'components/formik';
import { Modal } from 'components/modal';
import { useModal } from 'hooks';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useConnections, useDataLocations } from 'store/hooks/admin';
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
  IconButton,
  LabelPosition,
  OptionItem,
  Row,
  Show,
} from 'tno-core';
import { getSortableOptions } from 'utils';

import { defaultDataLocation } from './constants';
import { IDataLocationForm } from './interfaces';
import * as styled from './styled';
import { toForm, toModel } from './utils';

/** The page used to view and edit tags in the administrative section. */
export const DataLocationForm: React.FC = () => {
  const [, api] = useDataLocations();
  const { state } = useLocation();
  const [{ connections }, { findAllConnections }] = useConnections();
  const { id } = useParams();
  const navigate = useNavigate();
  const { toggle, isShowing } = useModal();

  const [loading, setLoading] = React.useState(true);
  const [dataLocation, setDataLocation] = React.useState<IDataLocationForm>(
    (state as any)?.dataLocation ?? defaultDataLocation,
  );

  const dataLocationId = Number(id);
  const connectionOptions = getSortableOptions(connections, [
    new OptionItem('This data location does not have a connection', ''),
  ]);

  React.useEffect(() => {
    if (!!dataLocationId && dataLocation?.id !== dataLocationId) {
      api.getDataLocation(dataLocationId).then((data) => {
        setDataLocation(toForm(data));
      });
    }
  }, [api, dataLocation?.id, dataLocationId]);

  React.useEffect(() => {
    if (loading && connections.length === 0) {
      setLoading(false);
      findAllConnections();
    }
  }, [connections.length, findAllConnections, loading]);

  const handleSubmit = async (values: IDataLocationForm) => {
    try {
      const originalId = values.id;
      const result = !dataLocation.id
        ? await api.addDataLocation(toModel(values))
        : await api.updateDataLocation(toModel(values));
      setDataLocation(toForm(result));
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/data/locations/${result.id}`);
    } catch {}
  };

  return (
    <styled.DataLocationForm>
      <IconButton
        iconType="back"
        label="Back to DataLocations"
        className="back-button"
        onClick={() => navigate('/admin/data/locations')}
      />
      <FormikForm
        initialValues={dataLocation}
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
                label="Connection"
                name="connectionId"
                tooltip="A connection provides a way for services to access files at this location"
                options={connectionOptions}
                onChange={(newValue: any) => {
                  const connection = connections.find((c) => c.id === newValue.value);
                  if (!!connection) setFieldValue('connection', connection);
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
              body="Are you sure you wish to remove this dataLocation?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteDataLocation(toModel(dataLocation));
                  toast.success(`${dataLocation.name} has successfully been deleted.`);
                  navigate('/admin/data/locations');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.DataLocationForm>
  );
};
