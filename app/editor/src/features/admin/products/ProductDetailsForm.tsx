import { useFormikContext } from 'formik';
import { debounce, noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useApp } from 'store/hooks';
import { useNotifications, useReports, useUsers } from 'store/hooks/admin';
import {
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getEnumStringOptions,
  getUserOptions,
  IOptionItem,
  IProductModel,
  OptionItem,
  ProductTypeName,
  Row,
  Show,
} from 'tno-core';

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
export const ProductDetailsForm: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IProductModel>();
  const [{ userInfo }] = useApp();
  const [{ users }, { findUsers }] = useUsers();

  const [{}, apiNotifications] = useNotifications();
  const [{}, apiReports] = useReports();
  const [targetProductOptions, setTargetProductOptions] = React.useState<IOptionItem[]>([]);

  const [userOptions, setUserOptions] = React.useState(getUserOptions(users.items));

  const productTypeOptions = getEnumStringOptions(ProductTypeName);

  React.useEffect(() => {
    if (userInfo?.id) {
      findUsers({ quantity: 50, includeUserId: values.ownerId })
        .then((results) => {
          setUserOptions(getUserOptions(results.items));
        })
        .catch(() => {});
    }
    // Only fire on initial load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userInfo?.id]);

  React.useEffect(() => {
    if (values?.productType && values?.targetProductId >= 0) {
      changeTargetProduct(values.productType);
    }
  }, [values?.productType, values?.targetProductId]);

  const handleFindUsers = debounce(async (text: string) => {
    const results = await findUsers({ quantity: 50, username: text }, true);
    setUserOptions(getUserOptions(results.items));
    return results;
  }, 500);

  const changeTargetProduct = React.useCallback(
    (targetProduct: ProductTypeName | undefined) => {
      if (!targetProduct) return;
      switch (targetProduct) {
        case ProductTypeName.Report:
          // set using reports
          apiReports.findAllReports().then((data) => {
            setTargetProductOptions(data.map((s) => new OptionItem(s.name, s.id)));
          });
          break;
        case ProductTypeName.EveningOverview:
          // set list to empty
          setTargetProductOptions([]);
          setFieldValue('targetProductId', -1);
          break;
        case ProductTypeName.Notification:
          // set using notifications
          apiNotifications.findAllNotifications().then((data) => {
            setTargetProductOptions(data.map((s) => new OptionItem(s.name, s.id)));
          });
          break;
      }
    },
    [targetProductOptions, apiReports, apiNotifications],
  );

  return (
    <>
      <Col className="form-inputs">
        <FormikText name="name" label="Name" placeholder="Enter unique product name" required />
        <FormikTextArea name="description" label="Description" />
        <FormikSelect
          name="productType"
          label="Product Type"
          required
          options={productTypeOptions}
          value={productTypeOptions.find((o) => o.value === values.productType)}
          onChange={(e: any) => changeTargetProduct(e?.value)}
        />
        <Show visible={targetProductOptions.length >= 1}>
          <FormikSelect
            name="targetProduct"
            label="Target Product"
            required={targetProductOptions.length >= 1}
            options={targetProductOptions}
            value={targetProductOptions.find((o) => o.value === values.targetProductId) || ''}
            onChange={(e: any) => {
              setFieldValue('targetProductId', e?.value);
            }}
          />
        </Show>
        <FormikSelect
          name="ownerId"
          label="Owner"
          options={userOptions}
          value={userOptions.find((u) => u.value === values.ownerId) || ''}
          onChange={(e) => {
            const option = e as OptionItem;
            setFieldValue(
              'values.ownerId',
              option?.value ? parseInt(option.value.toString()) : undefined,
            );
          }}
          onInputChange={(newValue) => {
            // TODO: Does not need to fire when an option is manually selected.
            handleFindUsers(newValue);
          }}
        />
        <Row alignItems="center">
          <FormikText
            width={FieldSize.Tiny}
            name="sortOrder"
            label="Sort Order"
            type="number"
            className="sort-order"
          />
          <FormikCheckbox label="Is Enabled" name="isEnabled" />
        </Row>
        <hr />
        <Row>
          <Col>
            <Show visible={!!values.id}>
              <Row>
                <FormikText width={FieldSize.Small} disabled name="updatedBy" label="Updated By" />
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
                <FormikText width={FieldSize.Small} disabled name="createdBy" label="Created By" />
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
        </Row>
      </Col>
    </>
  );
};
