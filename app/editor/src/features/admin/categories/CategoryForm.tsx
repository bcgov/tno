import { IconButton, LabelPosition } from 'components/form';
import { FormikCheckbox, FormikForm, FormikText, FormikTextArea } from 'components/formik';
import { FormikDatePicker } from 'components/formik/datepicker';
import { Modal } from 'components/modal';
import { ICategoryModel, useModal } from 'hooks';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useCategories } from 'store/hooks/admin/categories';
import { Button, ButtonVariant, Col, FieldSize, Row, Show } from 'tno-core';

import { defaultCategory } from './constants';
import * as styled from './styled';

/** The page used to view and edit tags in the administrative section. */
export const CategoryForm: React.FC = () => {
  const [, api] = useCategories();
  const { state } = useLocation();
  const { id } = useParams();
  const navigate = useNavigate();
  const categoryId = Number(id);
  const [category, setCategory] = React.useState<ICategoryModel>(
    (state as any)?.series ?? defaultCategory,
  );

  const { toggle, isShowing } = useModal();

  React.useEffect(() => {
    if (!!categoryId && category?.id !== categoryId) {
      api.getCategory(categoryId).then((data) => {
        setCategory(data);
      });
    }
  }, [api, category?.id, categoryId]);

  const handleSubmit = async (values: ICategoryModel) => {
    try {
      const originalId = values.id;
      const result = !category.id
        ? await api.addCategory(values)
        : await api.updateCategory(values);
      setCategory(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/categories/${result.id}`);
    } catch {}
  };

  return (
    <styled.CategoryForm>
      <IconButton
        iconType="back"
        label="Back to Categories"
        className="back-button"
        onClick={() => navigate('/admin/categories')}
      />
      <FormikForm
        initialValues={category}
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
                  await api.deleteCategory(category);
                  toast.success(`${category.name} has successfully been deleted.`);
                  navigate('/admin/media/types');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.CategoryForm>
  );
};
