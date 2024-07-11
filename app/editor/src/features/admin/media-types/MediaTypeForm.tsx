import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useMediaTypes } from 'store/hooks/admin';
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
  IMediaTypeModel,
  ListOptionName,
  Modal,
  OptionItem,
  Row,
  Show,
  useModal,
} from 'tno-core';

import { defaultMediaType } from './constants';
import * as styled from './styled';

/** The page used to view and edit tags in the administrative section. */
const MediaTypeForm: React.FC = () => {
  const [, api] = useMediaTypes();
  const { state } = useLocation();
  const { id } = useParams();
  const navigate = useNavigate();
  const { toggle, isShowing } = useModal();

  const mediaTypeId = Number(id);
  const [mediaType, setMediaType] = React.useState<IMediaTypeModel>(
    (state as any)?.mediaType ?? defaultMediaType,
  );
  const [settings, setSettings] = React.useState('');
  const listOptions = [
    new OptionItem('Source', ListOptionName.Source),
    new OptionItem('Program/Show', ListOptionName.Series),
  ];

  React.useEffect(() => {
    if (!!mediaTypeId && mediaType?.id !== mediaTypeId) {
      setMediaType({ ...defaultMediaType, id: mediaTypeId }); // Do this to stop double fetch.
      api.getMediaType(mediaTypeId).then((data) => {
        setMediaType(data);
        setSettings(JSON.stringify(data.settings, null, 2));
      });
    }
  }, [api, mediaType?.id, mediaTypeId]);

  React.useEffect(() => {
    try {
      const json = JSON.parse(settings);
      setMediaType((mediaType) => ({ ...mediaType, settings: json }));
      setSettings(JSON.stringify(json, null, 2));
    } catch {}
  }, [settings]);

  const handleSubmit = async (values: IMediaTypeModel) => {
    try {
      const originalId = values.id;
      const result = !mediaType.id
        ? await api.addMediaType(values)
        : await api.updateMediaType(values);
      setMediaType(result);
      toast.success(`${result.name} has successfully been saved.`);

      if (!originalId) navigate(`/admin/media-types/${result.id}`);
    } catch {}
  };

  return (
    <styled.MediaTypeForm>
      <IconButton
        iconType="back"
        label="Back to Media Types"
        className="back-button"
        onClick={() => navigate('/admin/media-types')}
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
              <FormikSelect label="List Option" name="listOption" options={listOptions} />
              <FormikText
                width={FieldSize.Tiny}
                name="sortOrder"
                label="Sort Order"
                type="number"
                className="sort-order"
              />
              <Col gap="0.5rem">
                <FormikCheckbox label="Is Enabled" name="isEnabled" />
                <FormikCheckbox label="Automatically transcribe when saved" name="autoTranscribe" />
              </Col>
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
              body="Are you sure you wish to remove this media type?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteMediaType(mediaType);
                  toast.success(`${mediaType.name} has successfully been deleted.`);
                  navigate('/admin/media-types');
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

export default MediaTypeForm;
