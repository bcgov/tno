import { getIn, useFormikContext } from 'formik';
import moment from 'moment';
import React from 'react';
import { useLookupOptions } from 'store/hooks';
import {
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  FormikTextArea,
  IFilterActionSettingsModel,
  IFilterSettingsModel,
  OptionItem,
  Row,
  Section,
  SentimentSlider,
  Show,
  ToggleGroup,
} from 'tno-core';

import { contentTypeOptions } from './constants';
import { ElasticQueryHelp } from './ElasticQueryHelp';
import { getActionOptions, getTagOptions } from './utils';

export interface IFilterSEttingsFormProps {
  /** Path to settings. */
  path?: string;
  /** Whether the keywords support elasticsearch query. */
  supportsElasticQuery?: boolean;
  /** Event fires when a field is changed. */
  onChange?: (values: IFilterSettingsModel) => void;
}

/**
 * Provides a common way to edit filter setting values.
 * @param param0 Component properties.
 * @returns Component form to edit filter setting values.
 */
export const FilterSettingsForm: React.FC<IFilterSEttingsFormProps> = ({
  path: defaultPath,
  supportsElasticQuery = true,
  onChange,
}) => {
  const { values, setFieldValue, setValues } = useFormikContext();
  const [{ mediaTypeOptions, sourceOptions, seriesOptions, contributorOptions, actions, tags }] =
    useLookupOptions();

  const [actionOptions, setActionOptions] = React.useState(getActionOptions(actions));
  const [tagOptions, setTagOptions] = React.useState(getTagOptions(tags));

  const settings: IFilterSettingsModel = defaultPath ? getIn(values, defaultPath) : values;
  const path = defaultPath ? `${defaultPath}.` : '';
  const startDate = settings.startDate ? moment(settings.startDate).format('YYYY/MM/DD') : '';
  const endDate = settings.endDate ? moment(settings.endDate).format('YYYY/MM/DD') : '';

  React.useEffect(() => {
    setActionOptions(getActionOptions(actions));
  }, [actions]);

  React.useEffect(() => {
    setTagOptions(getTagOptions(tags));
  }, [tags]);

  const updateSettings = React.useCallback(
    <T extends keyof IFilterSettingsModel>(key: T, value: any) => {
      var values = { ...settings };
      values[key] = value;
      if (key === 'dateOffset') {
        values = { ...values, startDate: undefined, endDate: undefined };
      } else if (key === 'startDate' || key === 'endDate') {
        values = { ...values, dateOffset: undefined };
      }
      if (defaultPath) setFieldValue(defaultPath, values);
      else setValues(values);
      onChange?.(values);
    },
    [defaultPath, onChange, setFieldValue, setValues, settings],
  );

  return (
    <>
      <FormikCheckbox name={`${path}searchUnpublished`} label="Search unpublished content" />
      <Row alignItems="center">
        <FormikText
          name={`${path}size`}
          label="Number of Stories"
          type="number"
          width="10ch"
          value={settings.size ?? 100}
          onChange={(e) => {
            const value = !!e.target.value ? parseInt(e.target.value) : 100;
            updateSettings('size', value);
          }}
        />
        <p>
          All filters must have a upward limit of content returned in a single request. The default
          is 10, the maximum is 10,000.
        </p>
      </Row>
      <Section>
        <Row gap="1rem" alignItems="center" className="frm-in">
          <label>Advanced Options:</label>
          <Row alignItems="center" justifyContent="space-between">
            Default operator:
            <ToggleGroup
              defaultSelected={settings.defaultOperator ?? 'and'}
              options={[
                {
                  id: 'and',
                  label: 'AND',
                  onClick: () => updateSettings('defaultOperator', 'and'),
                },
                {
                  id: 'or',
                  label: 'OR',
                  onClick: () => updateSettings('defaultOperator', 'or'),
                },
              ]}
            />
          </Row>
          <Row alignItems="center" justifyContent="space-between">
            Query Type:
            <ToggleGroup
              defaultSelected={settings.queryType ?? 'simple-query-string'}
              options={[
                {
                  id: 'query-string',
                  label: 'Advanced',
                  onClick: () => updateSettings('queryType', 'query-string'),
                },
                {
                  id: 'simple-query-string',
                  label: 'Simple',
                  onClick: () => updateSettings('queryType', 'simple-query-string'),
                },
              ]}
            />
          </Row>
        </Row>
        <Row nowrap>
          <Col flex="2">
            <FormikTextArea
              name={`${path}search`}
              label="Keywords"
              value={settings.search ?? ''}
              width={FieldSize.Large}
              rows={8}
              onChange={(e) => {
                const value = e.target.value.length ? e.target.value : undefined;
                updateSettings('search', value);
              }}
            />
          </Col>
          <Show visible={supportsElasticQuery}>
            <ElasticQueryHelp queryType={settings.queryType} />
          </Show>
        </Row>
        <Row gap="1rem">
          <label>Search for Keywords in: </label>
          <FormikCheckbox
            name={`${path}inHeadline`}
            label="Headline"
            onChange={(e) => updateSettings('inHeadline', e.target.checked)}
          />
          <FormikCheckbox
            name={`${path}inByline`}
            label="Byline"
            onChange={(e) => updateSettings('inByline', e.target.checked)}
          />
          <FormikCheckbox
            name={`${path}inStory`}
            label="Story text"
            onChange={(e) => updateSettings('inStory', e.target.checked)}
          />
          <FormikCheckbox
            name={`${path}inProgram`}
            label="Program"
            onChange={(e) => updateSettings('inProgram', e.target.checked)}
          />
        </Row>
      </Section>
      <Row nowrap>
        <Col>
          <FormikDatePicker
            name={`${path}startDate`}
            label="Start Date"
            value={startDate}
            selectedDate={startDate}
            width="15ch"
            onChange={(value) => {
              updateSettings('startDate', value);
            }}
            disabled={!!settings.dateOffset}
            isClearable={true}
          />
        </Col>
        <Col>
          <FormikDatePicker
            name={`${path}endDate`}
            label="End Date"
            value={endDate}
            selectedDate={endDate}
            width="15ch"
            onChange={(value) => {
              updateSettings('endDate', value);
            }}
            disabled={!!settings.dateOffset}
            isClearable={true}
          />
        </Col>
        <Row nowrap>
          <Col justifyContent="center" className="frm-in pad-05">
            <label>Or</label>
          </Col>
          <FormikText
            name={`${path}dateOffset`}
            label="Date Offset"
            type="number"
            width="10ch"
            value={settings.dateOffset ?? ''}
            onChange={(e) => {
              const value = !!e.target.value ? parseInt(e.target.value) : undefined;
              updateSettings('dateOffset', value);
            }}
            disabled={!!settings.startDate || !!settings.endDate}
          />
          <p>
            A date offset provides a consistent way to search for content that was published
            relative to today's date.
          </p>
        </Row>
      </Row>
      <Row>
        <Col flex="1">
          <FormikText
            name={`${path}edition`}
            label="Edition"
            value={settings.edition ?? ''}
            onChange={(e) => {
              const value = e.target.value.length ? e.target.value : undefined;
              updateSettings('edition', value);
            }}
          />
        </Col>
        <Col flex="1">
          <FormikText
            name={`${path}section`}
            label="Section"
            value={settings.section ?? ''}
            onChange={(e) => {
              const value = e.target.value.length ? e.target.value : undefined;
              updateSettings('section', value);
            }}
          />
        </Col>
        <Col flex="1">
          <FormikText
            name={`${path}page`}
            label="Page"
            value={settings.page ?? ''}
            onChange={(e) => {
              const value = e.target.value.length ? e.target.value : undefined;
              updateSettings('page', value);
            }}
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <FormikSelect
            name={`${path}sourceIds`}
            label="Sources"
            isMulti
            options={sourceOptions}
            value={
              sourceOptions.filter((mt) =>
                settings.sourceIds?.some((p: number) => p === mt.value),
              ) ?? []
            }
            onChange={(newValue: any) => {
              const sourceIds = newValue.map((v: OptionItem) => v.value);
              updateSettings('sourceIds', sourceIds);
            }}
          />
        </Col>
        <Col flex="1">
          <FormikSelect
            name={`${path}mediaTypeIds`}
            label="Media Types"
            isMulti
            options={mediaTypeOptions}
            value={
              mediaTypeOptions.filter((mt) =>
                settings.mediaTypeIds?.some((p: number) => p === mt.value),
              ) ?? []
            }
            onChange={(newValue: any) => {
              const mediaTypeIds = newValue.map((v: OptionItem) => v.value);
              updateSettings('mediaTypeIds', mediaTypeIds);
            }}
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <FormikSelect
            name={`${path}seriesIds`}
            label="Programs/Shows"
            isMulti
            options={seriesOptions}
            value={
              seriesOptions.filter((mt) =>
                settings.seriesIds?.some((p: number) => p === mt.value),
              ) ?? []
            }
            onChange={(newValue: any) => {
              const seriesIds = newValue.map((v: OptionItem) => v.value);
              updateSettings('seriesIds', seriesIds);
            }}
          />
        </Col>
        <Col flex="1">
          <FormikSelect
            name={`${path}contributorIds`}
            label="Columnists/Anchors"
            isMulti
            options={contributorOptions}
            value={
              contributorOptions.filter((mt) =>
                settings.contributorIds?.some((p: number) => p === mt.value),
              ) ?? []
            }
            onChange={(newValue: any) => {
              const contributorIds = newValue.map((v: OptionItem) => v.value);
              updateSettings('contributorIds', contributorIds);
            }}
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <FormikSelect
            name={`${path}actions`}
            label="Actions"
            isMulti
            options={actionOptions}
            value={
              actionOptions.filter((mt) =>
                settings.actions?.some((p: IFilterActionSettingsModel) => p.id === mt.value),
              ) ?? []
            }
            onChange={(newValue: any) => {
              const actions = newValue.map((o: OptionItem) => ({
                id: o.value,
                valueType: o.label === 'Commentary' ? 'String' : 'Boolean',
                value: o.label === 'Commentary' ? '*' : 'true',
              }));
              updateSettings('actions', actions);
            }}
          />
        </Col>
        <Col flex="1">
          <FormikSelect
            name={`${path}contentTypes`}
            label="Content Types"
            isMulti
            options={contentTypeOptions}
            value={
              contentTypeOptions.filter((mt) =>
                settings.contentTypes?.some((o: string) => o === mt.value),
              ) ?? []
            }
            onChange={(newValue: any) => {
              const contentTypes = newValue.map((o: OptionItem) => o.value);
              updateSettings('contentTypes', contentTypes);
            }}
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <FormikSelect
            name={`${path}tags`}
            label="Tags"
            isMulti
            options={tagOptions}
            value={
              tagOptions.filter((t) => settings.tags?.some((o: string) => o === t.value)) ?? []
            }
            onChange={(newValue: any) => {
              const tags = newValue.map((o: OptionItem) => o.value);
              updateSettings('tags', tags);
            }}
          />
        </Col>
        <Col flex="1">
          <SentimentSlider
            label="Sentiment"
            value={settings.sentiment ?? []}
            onChange={(value) => {
              updateSettings('sentiment', value);
            }}
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <div className="frm-in">
            <label data-tooltip-id="main-tooltip">Topics </label>
            <FormikCheckbox
              name={`${path}hasTopic`}
              label="Has at least one Topic set"
              onChange={(e) => updateSettings('hasTopic', e.target.checked)}
            />
          </div>
        </Col>
      </Row>
    </>
  );
};
