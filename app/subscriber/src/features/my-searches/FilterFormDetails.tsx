import { useFormikContext } from 'formik';
import moment from 'moment';
import React from 'react';
import { useLookupOptions } from 'store/hooks';
import { getActionOptions } from 'store/hooks/subscriber/getActionOptions';
import { getTagOptions } from 'store/hooks/subscriber/getTagOptions';
import {
  Checkbox,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikText,
  FormikTextArea,
  generateQuery,
  IFilterActionSettingsModel,
  IFilterModel,
  IFilterSettingsModel,
  OptionItem,
  Row,
  Select,
  SentimentSlider,
} from 'tno-core';

import { contentTypeOptions } from './constants';

/**
 * The page used to view and edit filters.
 * @returns Component.
 */
export const FilterFormDetails: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IFilterModel>();

  const [{ productOptions, sourceOptions, seriesOptions, contributorOptions, actions, tags }] =
    useLookupOptions();

  const [, setFilter] = React.useState(JSON.stringify(values.query, null, 2));
  const [actionOptions, setActionOptions] = React.useState(getActionOptions(actions));
  const [tagOptions, setTagOptions] = React.useState(getTagOptions(tags));

  const startDate = values.settings.startDate
    ? moment(values.settings.startDate).format('YYYY/MM/DD')
    : '';
  const endDate = values.settings.endDate
    ? moment(values.settings.endDate).format('YYYY/MM/DD')
    : '';

  React.useEffect(() => {
    setActionOptions(getActionOptions(actions));
  }, [actions]);

  React.useEffect(() => {
    setTagOptions(getTagOptions(tags));
  }, [tags]);

  const updateQuery = React.useCallback(
    <T extends keyof IFilterSettingsModel>(key: T, value: any) => {
      var settings = { ...values.settings };
      settings[key] = value;
      if (key === 'dateOffset') {
        settings = { ...settings, startDate: undefined, endDate: undefined };
      } else if (key === 'startDate' || key === 'endDate') {
        settings = { ...settings, dateOffset: undefined };
      }
      const query = generateQuery(settings, values.query);
      setFieldValue('settings', settings);
      setFieldValue('query', query);
      setFilter(JSON.stringify(query, null, 2));
    },
    [setFieldValue, values.query, values.settings],
  );

  return (
    <Col className="form-inputs">
      <FormikText name="name" label="Name" required />
      <FormikTextArea name="description" label="Description" />
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
        <FormikText
          name="settings.size"
          label="Number of Stories"
          type="number"
          width="10ch"
          value={values.settings.size ?? 100}
          onChange={(e) => {
            const value = !!e.target.value ? parseInt(e.target.value) : 100;
            updateQuery('size', value);
          }}
        />
        <p>
          All filters must have a upward limit of content returned in a single request. The default
          limit is 10.
        </p>
      </Row>
      <Row className="search-in">
        <label>
          <b>Search for Keywords in: </b>
        </label>
        <Row className="checkboxes">
          <label>Headline</label>
          <Checkbox
            checked={values.settings.inHeadline ?? false}
            onChange={(e) => {
              updateQuery('inHeadline', e.target.checked);
            }}
          />
          <label>Byline</label>
          <Checkbox
            checked={values.settings.inByline ?? false}
            onChange={(e) => {
              updateQuery('inByline', e.target.checked);
            }}
          />
          <label>Story text</label>
          <Checkbox
            checked={values.settings.inStory ?? false}
            onChange={(e) => {
              updateQuery('inStory', e.target.checked);
            }}
          />
        </Row>
      </Row>
      <Row nowrap>
        <Col>
          <FormikDatePicker
            name="settings.startDate"
            label="Start Date"
            value={startDate}
            selectedDate={startDate}
            width="15ch"
            onChange={(value) => {
              updateQuery('startDate', value);
            }}
            isClearable={true}
          />
        </Col>
        <Col>
          <FormikDatePicker
            name="settings.endDate"
            label="End Date"
            value={endDate}
            selectedDate={endDate}
            width="15ch"
            onChange={(value) => {
              updateQuery('endDate', value);
            }}
            isClearable={true}
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <FormikText
            name="edition"
            label="Edition"
            value={values.settings.edition ?? ''}
            onChange={(e) => {
              const value = e.target.value.length ? e.target.value : undefined;
              updateQuery('edition', value);
            }}
          />
        </Col>
        <Col flex="1">
          <FormikText
            name="section"
            label="Section"
            value={values.settings.section ?? ''}
            onChange={(e) => {
              const value = e.target.value.length ? e.target.value : undefined;
              updateQuery('section', value);
            }}
          />
        </Col>
        <Col flex="1">
          <FormikText
            name="page"
            label="Page"
            value={values.settings.page ?? ''}
            onChange={(e) => {
              const value = e.target.value.length ? e.target.value : undefined;
              updateQuery('page', value);
            }}
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <Select
            name="sourceIds"
            label="Sources"
            isMulti
            options={sourceOptions}
            value={
              sourceOptions.filter((mt) =>
                values.settings.sourceIds?.some((p: number) => p === mt.value),
              ) ?? []
            }
            onChange={(newValue: any) => {
              const sourceIds = newValue.map((v: OptionItem) => v.value);
              updateQuery('sourceIds', sourceIds);
            }}
          />
        </Col>
        <Col flex="1">
          <Select
            name="productIds"
            label="Products"
            isMulti
            options={productOptions}
            value={
              productOptions.filter((mt) =>
                values.settings.productIds?.some((p: number) => p === mt.value),
              ) ?? []
            }
            onChange={(newValue: any) => {
              const productIds = newValue.map((v: OptionItem) => v.value);
              updateQuery('productIds', productIds);
            }}
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <Select
            name="seriesIds"
            label="Programs/Shows"
            isMulti
            options={seriesOptions}
            value={
              seriesOptions.filter((mt) =>
                values.settings.seriesIds?.some((p: number) => p === mt.value),
              ) ?? []
            }
            onChange={(newValue: any) => {
              const seriesIds = newValue.map((v: OptionItem) => v.value);
              updateQuery('seriesIds', seriesIds);
            }}
          />
        </Col>
        <Col flex="1">
          <Select
            name="contributorIds"
            label="Columnists/Anchors"
            isMulti
            options={contributorOptions}
            value={
              contributorOptions.filter((mt) =>
                values.settings.contributorIds?.some((p: number) => p === mt.value),
              ) ?? []
            }
            onChange={(newValue: any) => {
              const contributorIds = newValue.map((v: OptionItem) => v.value);
              updateQuery('contributorIds', contributorIds);
            }}
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <Select
            name="actions"
            label="Actions"
            isMulti
            options={actionOptions}
            value={
              actionOptions.filter((mt) =>
                values.settings.actions?.some((p: IFilterActionSettingsModel) => p.id === mt.value),
              ) ?? []
            }
            onChange={(newValue: any) => {
              const actions = newValue.map((o: OptionItem) => ({
                id: o.value,
                valueType: o.label === 'Commentary' ? 'String' : 'Boolean',
                value: o.label === 'Commentary' ? '*' : 'true',
              }));
              updateQuery('actions', actions);
            }}
          />
        </Col>
        <Col flex="1">
          <Select
            name="contentTypes"
            label="Content Types"
            isMulti
            options={contentTypeOptions}
            value={
              contentTypeOptions.filter((mt) =>
                values.settings.contentTypes?.some((o: string) => o === mt.value),
              ) ?? []
            }
            onChange={(newValue: any) => {
              const contentTypes = newValue.map((o: OptionItem) => o.value);
              updateQuery('contentTypes', contentTypes);
            }}
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <Select
            name="tags"
            label="Tags"
            isMulti
            options={tagOptions}
            value={
              tagOptions.filter((t) => values.settings.tags?.some((o: string) => o === t.value)) ??
              []
            }
            onChange={(newValue: any) => {
              const tags = newValue.map((o: OptionItem) => o.value);
              updateQuery('tags', tags);
            }}
          />
        </Col>
        <Col flex="1">
          <SentimentSlider
            label="Sentiment"
            value={values.settings.sentiment ?? []}
            onChange={(value) => {
              updateQuery('sentiment', value);
            }}
          />
        </Col>
      </Row>
    </Col>
  );
};
