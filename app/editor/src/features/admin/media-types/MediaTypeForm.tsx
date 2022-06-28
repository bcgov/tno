import { FieldSize, IconButton, LabelPosition } from 'components/form';
import { FormikCheckbox, FormikForm, FormikText, FormikTextArea } from 'components/formik';
import { FormikDatePicker } from 'components/formik/datepicker';
import { Modal } from 'components/modal';
import { useModal } from 'hooks';
import { IMediaTypeModel } from 'hooks/api-editor';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useMediaTypes } from 'store/hooks/admin';
import { Button, ButtonVariant, Col, Row, Show } from 'tno-core';

import { defaultMediaType } from './constants';
import * as styled from './styled';

/** The page used to view and edit media types in the administrative section. */
export const MediaTypeForm: React.FC = () => {
  const [, api] = useMediaTypes();
  const { state } = useLocation();
  const { id } = useParams();
  const navigate = useNavigate();
  const mediaTypeId = Number(id);
  const [mediaType, setMediaType] = React.useState<IMediaTypeModel>(
    (state as any)?.mediaType ?? defaultMediaType,
  );

  const { toggle, isShowing } = useModal();

  React.useEffect(() => {
    if (!!mediaTypeId && mediaType?.id !== mediaTypeId) {
      api.getMediaType(mediaTypeId).then((data) => {
        setMediaType(data);
      });
    }
  }, [api, mediaType?.id, mediaTypeId]);

  const handleSubmit = async (values: IMediaTypeModel) => {
    try {
      const originalId = values.id;
      const result = !mediaType.id
        ? await api.addMediaType(values)
        : await api.updateMediaType(values);
      setMediaType(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/media/types/${result.id}`);
    } catch (err: any) {
      if (err.response.data.details.includes('duplicate key value violates unique constraint')) {
        toast.error(`The name ${values.name} is already in use, please choose another.`);
      }
    }
  };

  return (
    <styled.MediaTypeForm>
      <IconButton
        iconType="back"
        label="Back to Media Types"
        className="back-button"
        onClick={() => navigate('/admin/media/types')}
      />
      <FormikForm
        initialValues={mediaType}
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
              body="Are you sure you wish to remove this media type?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteMediaType(mediaType);
                  toast.success(`${mediaType.name} has successfully been deleted.`);
                  navigate('/admin/media/types');
                } catch (err: any) {
                  console.log(err);
                  if (err.response.data.details.includes('is still referenced from table')) {
                    toast.error(
                      `${mediaType.name} is referenced by a data source and cannot be deleted.`,
                    );
                  }
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.MediaTypeForm>
  );
};
