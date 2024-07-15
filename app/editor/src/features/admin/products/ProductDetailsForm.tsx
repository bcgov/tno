import { useFormikContext } from 'formik';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useNotifications, useReports } from 'store/hooks/admin';
import {
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getEnumStringOptions,
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

  const [, apiNotifications] = useNotifications();
  const [, apiReports] = useReports();
  const [targetProductOptions, setTargetProductOptions] = React.useState<IOptionItem[]>([]);

  const productTypeOptions = getEnumStringOptions(ProductTypeName);

  const changeTargetProduct = React.useCallback(
    (targetProduct: ProductTypeName | undefined) => {
      if (!targetProduct) return;
      switch (targetProduct) {
        case ProductTypeName.Report:
          // set using reports
          apiReports.findAllReportsHeadersOnly().then((data) => {
            setTargetProductOptions(
              data.map((s) => new OptionItem(`${s.name} - [${s.owner?.displayName}]`, s.id)),
            );
          });
          break;
        case ProductTypeName.EveningOverview:
          // set list to empty
          setTargetProductOptions([]);
          setFieldValue('targetProductId', -1);
          break;
        case ProductTypeName.Notification:
          // set using notifications
          apiNotifications.findNotifications().then((data) => {
            setTargetProductOptions(data.map((s) => new OptionItem(s.name, s.id)));
          });
          break;
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [apiReports, apiNotifications], // KGM: Skipped 'setFieldValue' to avoid circular dependecy issue
  );

  React.useEffect(() => {
    if (values?.productType && values?.targetProductId >= 0) {
      changeTargetProduct(values.productType);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [values?.productType, values?.targetProductId]); // KGM: Skipped 'changeTargetProduct' to avoid circular dependecy issue

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
        <Row alignItems="center">
          <FormikCheckbox
            label="Is Public"
            name="isPublic"
            tooltip="A public report will show up in the list for ALL subscribers to see and request access
            to."
          />
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
