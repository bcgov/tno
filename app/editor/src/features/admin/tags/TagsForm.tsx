import { FormikForm } from 'components/formik';
import { Modal } from 'components/modal';
import { useModal } from 'hooks';
import { ITagModel } from 'hooks/api-editor';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useTags } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikText,
  FormikTextArea,
  IconButton,
  LabelPosition,
  Row,
  Show,
} from 'tno-core';

import { useTooltips } from './../../../hooks/useTooltips';
import { defaultTag } from './constants';
import * as styled from './styled';

/** The page used to view and edit tags in the administrative section. */
export const TagsForm: React.FC = () => {
  const [, api] = useTags();
  const { state } = useLocation();
  const { id } = useParams();
  const navigate = useNavigate();
  useTooltips();

  const [tag, setTag] = React.useState<ITagModel>((state as any)?.tag ?? defaultTag);

  const { toggle, isShowing } = useModal();

  React.useEffect(() => {
    if (!!id && tag?.id !== id && id !== '***') {
      api.getTag(id).then((data) => {
        setTag(data);
      });
    }
  }, [api, tag?.id, id]);

  const handleSubmit = async (values: ITagModel) => {
    try {
      const originalId = values.id;
      const result = id === '***' ? await api.addTag(values) : await api.updateTag(values);
      setTag(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (originalId !== values.id) navigate(`/admin/tags/${result.id}`);
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
        {({ isSubmitting, values, errors }) => (
          <div className="form-container">
            <Col className="form-inputs">
              <FormikText
                width={FieldSize.Large}
                name="id"
                label="Code"
                disabled={id !== '***'}
                tooltip="Cannot change the tag code after it has been created"
              />
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
              body="Are you sure you wish to remove this tag?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteTag(tag);
                  toast.success(`${tag.name} has successfully been deleted.`);
                  navigate('/admin/tags');
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
