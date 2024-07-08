import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { FormikForm } from 'components/formik';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp } from 'store/hooks';
import { useProducts } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  IconButton,
  IProductModel,
  Modal,
  Row,
  Show,
  Tab,
  Tabs,
  useModal,
} from 'tno-core';

import { defaultProduct } from './constants';
import { ProductDetailsForm } from './ProductDetailsForm';
import { ProductSubRequests } from './ProductSubRequests';
import { ProductSubscribersForm } from './ProductSubscribersForm';
import * as styled from './styled';

/**
 * The page used to view and edit Products.
 * @returns Component.
 */
const ProductForm: React.FC = () => {
  const navigate = useNavigate();
  const [{ userInfo }] = useApp();
  const { id } = useParams();
  const [, { getProduct, addProduct, updateProduct, deleteProduct }] = useProducts();
  const { state } = useLocation();
  const { toggle, isShowing } = useModal();

  const [Product, setProduct] = React.useState<IProductModel>(
    (state as any)?.Product ?? { ...defaultProduct, ownerId: userInfo?.id },
  );
  const [active, setActive] = React.useState('Product');

  const ProductId = Number(id);

  React.useEffect(() => {
    if (!!ProductId && Product?.id !== ProductId) {
      setProduct({ ...defaultProduct, id: ProductId }); // Do this to stop double fetch.
      getProduct(ProductId).then((data) => {
        setProduct(data);
      });
    }
  }, [getProduct, Product?.id, ProductId]);

  const handleSubmit = React.useCallback(
    async (values: IProductModel) => {
      try {
        const originalId = values.id;
        const result = !Product.id ? await addProduct(values) : await updateProduct(values);
        setProduct(result);
        toast.success(`${result.name} has successfully been saved.`);

        if (!originalId) navigate(`/admin/products/${result.id}`);
      } catch {}
    },
    [Product.id, addProduct, navigate, updateProduct],
  );

  return (
    <styled.ProductForm>
      <IconButton
        iconType="back"
        label="Back to Products"
        className="back-button"
        onClick={() => navigate('/admin/products')}
      />
      <FormikForm
        initialValues={Product}
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
                  label="Product"
                  onClick={() => {
                    setActive('Product');
                  }}
                  active={active === 'Product'}
                />
                <Tab
                  label="Subscribers"
                  onClick={() => {
                    setActive('subscribers');
                  }}
                  active={active === 'subscribers'}
                />
                <Tab
                  label="Requests"
                  onClick={() => {
                    setActive('Requests');
                  }}
                  active={active === 'Requests'}
                />
              </>
            }
          >
            <div className="form-container">
              <Show visible={active === 'Product'}>
                <ProductDetailsForm />
              </Show>
              <Show visible={active === 'subscribers'}>
                <ProductSubscribersForm />
              </Show>
              <Show visible={active === 'Requests'}>
                <ProductSubRequests />
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
                body="Are you sure you wish to remove this product?"
                isShowing={isShowing}
                hide={toggle}
                type="delete"
                confirmText="Yes, Remove It"
                onConfirm={async () => {
                  try {
                    await deleteProduct(Product);
                    toast.success(`${Product.name} has successfully been deleted.`);
                    navigate('/admin/products');
                  } finally {
                    toggle();
                  }
                }}
              />
            </div>
          </Tabs>
        )}
      </FormikForm>
    </styled.ProductForm>
  );
};

export default ProductForm;
