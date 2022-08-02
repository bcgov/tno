import { IconButton, LabelPosition } from 'components/form';
import { FormikCheckbox, FormikForm, FormikText, FormikTextArea } from 'components/formik';
import { FormikDatePicker } from 'components/formik/datepicker';
import { Modal } from 'components/modal';
import { ITagModel, useModal } from 'hooks';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useTags } from 'store/hooks/admin';
import { Button, ButtonVariant, Col, FieldSize, Row, Show } from 'tno-core';

import { defaultTag } from './constants';
import * as styled from './styled';

/** The page used to view and edit tags in the administrative section. */
export const TagsForm: React.FC = () => {
  const [, api] = useTags();
  const { state } = useLocation();
  const { id } = useParams();
  const navigate = useNavigate();
  const tagId = id;
  const [tag, setTag] = React.useState<ITagModel>((state as any)?.series ?? defaultTag);

  const { toggle, isShowing } = useModal();

  React.useEffect(() => {
    if (!!tagId && tag?.id !== tagId) {
      api.getTag(tagId).then((data) => {
        setTag(data);
      });
    }
  }, [api, tag?.id, tagId]);

  const handleSubmit = async (values: ITagModel) => {
    try {
      const originalId = values.id;
      const result = !tag.id ? await api.addTag(values) : await api.updateTag(values);
      setTag(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/tags/${result.id}`);
    } catch {}
  };

  return (
    <styled.TagsForm>
      <IconButton
        iconType="back"
        label="Back to Tags"
        className="back-button"
        onClick={() => navigate('/admin/tags')}
      />
      <FormikForm
        initialValues={tag}
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
                  await api.deleteTag(tag);
                  toast.success(`${tag.name} has successfully been deleted.`);
                  navigate('/admin/media/types');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.TagsForm>
  );
};
