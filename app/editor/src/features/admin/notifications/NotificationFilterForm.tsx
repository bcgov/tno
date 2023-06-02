import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { useLookupOptions } from 'store/hooks';
import { Col, FormikSelect, FormikText, INotificationModel, IOptionItem, Row } from 'tno-core';

export const NotificationFilterForm = () => {
  const { values, setFieldValue } = useFormikContext<INotificationModel>();
  const [{ productOptions, sourceOptions }] = useLookupOptions();

  const [filter, setFilter] = React.useState(JSON.stringify(values.filter, null, 2));

  React.useEffect(() => {
    setFilter(JSON.stringify(values.filter, null, 2));
  }, [values.filter]);

  return (
    <Col>
      <p>
        A filter provides a way to limit which notifications are sent out to subscribers. Each time
        an event triggers this notification it will validate that the content passes this filter.
      </p>
      <Row>
        <FormikText
          name="filter.headline"
          label="Headline"
          onChange={(e) =>
            setFieldValue('filter.headline', e.target.value ? e.target.value : undefined)
          }
        />
        <FormikText
          name="filter.page"
          label="Page"
          onChange={(e) =>
            setFieldValue('filter.page', e.target.value ? e.target.value : undefined)
          }
        />
        <FormikText
          name="filter.section"
          label="Section"
          onChange={(e) =>
            setFieldValue('filter.section', e.target.value ? e.target.value : undefined)
          }
        />
        <FormikText
          name="filter.edition"
          label="Edition"
          onChange={(e) =>
            setFieldValue('filter.edition', e.target.value ? e.target.value : undefined)
          }
        />
        <FormikText
          name="filter.byline"
          label="Byline"
          onChange={(e) =>
            setFieldValue('filter.byline', e.target.value ? e.target.value : undefined)
          }
        />
      </Row>
      <Row>
        <FormikSelect
          name="filter.sourceIds"
          label="Sources"
          isMulti
          options={sourceOptions}
          value={sourceOptions.filter((option) => {
            return values.filter.sourceIds?.find((id: number) => id === option.value) ?? false;
          })}
          onChange={(selectedValues) => {
            const selected = selectedValues as IOptionItem[];
            setFieldValue(
              'filter.sourceIds',
              selected.map((s) => s.value),
            );
          }}
        />
        <FormikText
          name="filter.otherSource"
          label="Other Source"
          onChange={(e) =>
            setFieldValue('filter.otherSource', e.target.value ? e.target.value : undefined)
          }
        />
      </Row>
      <Row>
        <FormikSelect
          name="filter.productIds"
          label="Products"
          isMulti
          options={productOptions}
          value={productOptions.filter((option) => {
            return values.filter.productIds?.find((id: number) => id === option.value) ?? false;
          })}
          onChange={(selectedValues) => {
            const selected = selectedValues as IOptionItem[];
            setFieldValue(
              'filter.productIds',
              selected.map((s) => s.value),
            );
          }}
        />
        <FormikText
          name="filter.product"
          label="Product"
          onChange={(e) =>
            setFieldValue('filter.product', e.target.value ? e.target.value : undefined)
          }
        />
      </Row>
      <Col className="code frm-in">
        <label htmlFor="txa-filter">JSON Filter</label>
        <Col className="editor">
          <Editor
            id="txa-filter"
            value={filter}
            onValueChange={(code) => {
              setFilter(code);
              try {
                const json = JSON.parse(code);
                setFieldValue('filter', json);
              } catch {
                // Ignore errors.
                // TODO: Inform user of formatting issues on blur/validation.
              }
            }}
            highlight={(code) => {
              return highlight(code, languages.json, 'json');
            }}
          />
        </Col>
      </Col>
    </Col>
  );
};
