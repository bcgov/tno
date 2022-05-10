import { FieldSize, IconButton, LabelPosition } from 'components/form';
import { FormikCheckbox, FormikForm, FormikText, FormikTextArea } from 'components/formik';
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
              <FormikTextArea name="description" label="Description" width={FieldSize.Medium} />
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
                  toast.success(`${mediaType.name} has successfully been deleted.`);
                  navigate('/admin/media/types');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.MediaType>
  );
};
