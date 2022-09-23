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
import { useModal, useTooltips } from 'hooks';
import { IIngestTypeModel } from 'hooks/api-editor';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useIngestTypes } from 'store/hooks/admin';
import { Button, ButtonVariant, Col, FieldSize, Row, Show } from 'tno-core';

import { contentTypeOptions, defaultIngestType } from './constants';
import * as styled from './styled';

/** The page used to view and edit ingest types in the administrative section. */
export const IngestTypeForm: React.FC = () => {
  const [, api] = useIngestTypes();
  const { state } = useLocation();
  const { id } = useParams();
  const navigate = useNavigate();
  useTooltips();

  const ingestTypeId = Number(id);
  const [ingestType, setIngestType] = React.useState<IIngestTypeModel>(
    (state as any)?.ingestType ?? defaultIngestType,
  );

  const { toggle, isShowing } = useModal();

  React.useEffect(() => {
    if (!!ingestTypeId && ingestType?.id !== ingestTypeId) {
      api.getIngestType(ingestTypeId).then((data) => {
        setIngestType(data);
      });
    }
  }, [api, ingestType?.id, ingestTypeId]);

  const handleSubmit = async (values: IIngestTypeModel) => {
    try {
      const originalId = values.id;
      const result = !ingestType.id
        ? await api.addIngestType(values)
        : await api.updateIngestType(values);
      setIngestType(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/ingest/types/${result.id}`);
    } catch {}
  };

  return (
    <styled.IngestTypeForm>
      <IconButton
        iconType="back"
        label="Back to Ingest Types"
        className="back-button"
        onClick={() => navigate('/admin/ingest/types')}
      />
      <FormikForm
        initialValues={ingestType}
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
                name="contentType"
                label="Content Type"
                tooltip="Controls which form is used for this type of content"
                width={FieldSize.Big}
                value={contentTypeOptions.find((o) => o.value === values.contentType) ?? ''}
                onChange={(newValue: any) => {
                  setFieldValue('contentType', newValue.value);
                }}
                options={contentTypeOptions}
                required
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
              body="Are you sure you wish to remove this ingest type?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteIngestType(ingestType);
                  toast.success(`${ingestType.name} has successfully been deleted.`);
                  navigate('/admin/ingest/types');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.IngestTypeForm>
  );
};
