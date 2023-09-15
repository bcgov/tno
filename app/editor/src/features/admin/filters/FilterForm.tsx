import { FormikForm } from 'components/formik';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp } from 'store/hooks';
import { useFilters } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  IconButton,
  IFilterModel,
  Modal,
  Row,
  Show,
  Tab,
  Tabs,
  useModal,
} from 'tno-core';

import { defaultFilter } from './constants';
import { FilterFormDetails } from './FilterFormDetails';
import { FilterFormPreview } from './FilterFormPreview';
import { FilterFormQuery } from './FilterFormQuery';
import * as styled from './styled';

export interface IFilterFormProps {}

/**
 * Provides a page to admin a list of filters and filter templates.
 * @returns Component to admin filters and filter templates.
 */
const FilterForm: React.FC<IFilterFormProps> = () => {
  const navigate = useNavigate();
  const [{ userInfo }] = useApp();
  const { id } = useParams();
  const [, { addFilter, deleteFilter, getFilter, updateFilter }] = useFilters();
  const { toggle, isShowing } = useModal();

  const [active, setActive] = React.useState('details');
  const [filter, setFilter] = React.useState<IFilterModel>({
    ...defaultFilter,
    ownerId: userInfo?.id ?? 0,
  });

  const filterId = Number(id);

  React.useEffect(() => {
    if (!!filterId && filter?.id !== filterId) {
      setFilter({ ...defaultFilter, id: filterId }); // Do this to stop double fetch.
      getFilter(filterId).then((data) => {
        setFilter(data);
      });
    }
  }, [getFilter, filter?.id, filterId]);

  const handleSubmit = async (values: IFilterModel) => {
    try {
      const originalId = values.id;
      const result = !filter.id ? await addFilter(values) : await updateFilter(values);
      setFilter(result);

      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/filters/${result.id}`);
    } catch {}
  };

  return (
    <styled.FilterForm>
      <IconButton
        iconType="back"
        label="Back to filters"
        className="back-button"
        onClick={() => navigate('/admin/filters')}
      />
      <FormikForm
        initialValues={filter}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values, setFieldValue }) => (
          <Tabs
            tabs={
              <>
                <Tab
                  label="Details"
                  onClick={() => {
                    setActive('details');
                  }}
                  active={active === 'details'}
                />
                <Tab
                  label="Query"
                  onClick={() => {
                    setActive('query');
                  }}
                  active={active === 'query'}
                />
                <Tab
                  label="Preview"
                  onClick={() => {
                    setActive('preview');
                  }}
                  active={active === 'preview'}
                />
              </>
            }
          >
            <div className="form-container">
              <Show visible={active === 'details'}>
                <FilterFormDetails />
              </Show>
              <Show visible={active === 'query'}>
                <FilterFormQuery />
              </Show>
              <Show visible={active === 'preview'}>
                <FilterFormPreview />
              </Show>
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
                body="Are you sure you wish to remove this filter?"
                isShowing={isShowing}
                hide={toggle}
                type="delete"
                confirmText="Yes, Remove It"
                onConfirm={async () => {
                  try {
                    await deleteFilter(filter);
                    toast.success(`${filter.name} has successfully been deleted.`);
                    navigate('/admin/filters');
                  } catch {
                    // Globally handled
                  } finally {
                    toggle();
                  }
                }}
              />
            </div>
          </Tabs>
        )}
      </FormikForm>
    </styled.FilterForm>
  );
};

export default FilterForm;
