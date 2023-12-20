import { FormikForm } from 'components/formik';
import { SearchWithLogout } from 'components/search-with-logout';
import React from 'react';
import { FaArrowLeft } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp, useFilters } from 'store/hooks';
import { Button, ButtonVariant, IFilterModel, Modal, Row, Show, useModal } from 'tno-core';

import { defaultFilter } from './constants';
import { FilterFormDetails } from './FilterFormDetails';
import * as styled from './styled';

export interface IFilterFormProps {}

/**
 * Provides a page to configure a filter.
 * @returns Component
 */
const FilterForm: React.FC<IFilterFormProps> = () => {
  const navigate = useNavigate();
  const [{ userInfo }] = useApp();
  const { id } = useParams();
  const [, { addFilter, deleteFilter, getFilter, updateFilter }] = useFilters();
  const { toggle, isShowing } = useModal();

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
      if (!originalId) navigate(`/filters/${result.id}`);
    } catch {}
  };

  return (
    <styled.SearchForm>
      <SearchWithLogout />
      <div className="main">
        <Row className="header">
          <FaArrowLeft className="back-arrow" onClick={() => navigate(-1)} />
          <div className="title">{`Manage filter`}</div>
        </Row>
        <FormikForm
          initialValues={filter}
          onSubmit={(values, { setSubmitting }) => {
            handleSubmit(values);
            setSubmitting(false);
          }}
        >
          {({ isSubmitting, values }) => (
            <div className="form-container">
              <FilterFormDetails />
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
          )}
        </FormikForm>
      </div>
    </styled.SearchForm>
  );
};

export default FilterForm;
