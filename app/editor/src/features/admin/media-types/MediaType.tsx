import { FieldSize, IconButton, LabelPosition } from 'components/form';
import { FormikCheckbox, FormikForm, FormikText } from 'components/formik';
import { Modal } from 'components/modal';
import { useModal } from 'hooks';
import { IMediaTypeModel } from 'hooks/api-editor';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useMediaTypes } from 'store/hooks/admin';
import { Button, ButtonVariant } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex';

import { defaultMediaType } from './constants';
import * as styled from './styled';

/** The page used to view and edit media types in the administrative section. */
export const MediaType: React.FC = () => {
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
    if (mediaType?.id !== mediaTypeId) {
      api.getMediaType(mediaTypeId).then((data) => {
        setMediaType(data);
      });
    }
  }, [api, mediaType?.id, mediaTypeId]);

  const handleSubmit = async (values: IMediaTypeModel) => {
    try {
      if (!!mediaType.id) {
        const data = await api.updateMediaType(values);
        setMediaType(data);
        toast.success(`${data.name} has successfully been saved.`);
      } else {
        const data = await api.addMediaType(values);
        setMediaType(data);
        toast.success(`${data.name} has successfully been created.`);
        navigate(`/admin/media/types/${data.id}`);
      }
    } catch {}
  };

  return (
    <styled.MediaType>
      <IconButton
        iconType="back"
        label="Back to Media Types"
        className="back-button"
        onClick={() => navigate('/admin/media/types')}
      />
      <FormikForm
        initialValues={mediaType}
        validate={(values) => {
          const errors = {};
          return errors;
        }}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ errors }) => (
          <div className="form-container">
            <Row className="form-inputs">
              <FormikText name="name" label="Name" />
            </Row>
            <Row>
              <FormikText name="description" label="Description" />
            </Row>
            <Row>
              <FormikText
                width={FieldSize.Tiny}
                name="sortOrder"
                label="Sort Order"
                type="number"
                className="sord-order"
              />
            </Row>
            <Row className="form-inputs">
              <FormikCheckbox
                labelPosition={LabelPosition.Top}
                label="Is Enabled"
                name="isEnabled"
              />
            </Row>
            <Row justify="center" className="form-inputs">
              <Button variant={ButtonVariant.action} type="submit">
                Save
              </Button>
              <Button onClick={toggle} variant={ButtonVariant.danger}>
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
                } finally {
                  toggle();
                  toast.success(`${mediaType.name} has successfully been deleted.`);
                  navigate('/admin/media/types');
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.MediaType>
  );
};
