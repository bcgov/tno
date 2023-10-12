import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { useLookupOptions } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikText,
  FormikTextArea,
  generateQuery,
  IFilterModel,
  OptionItem,
  Row,
  Select,
  SentimentSlider,
} from 'tno-core';

import { contentTypeOptions } from './constants';
import { getActionOptions, getTagOptions } from './utils';

/**
 * The page used to view and edit report filter.
 * @returns Component.
 */
export const FilterFormQuery: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IFilterModel>();
  const [{ productOptions, sourceOptions, seriesOptions, contributorOptions, actions, tags }] =
    useLookupOptions();

  const [filter, setFilter] = React.useState(JSON.stringify(values.query, null, 2));
  const [actionOptions, setActionOptions] = React.useState(getActionOptions(actions));
  const [tagOptions, setTagOptions] = React.useState(getTagOptions(tags));

  React.useEffect(() => {
    setActionOptions(getActionOptions(actions));
  }, [actions]);

  React.useEffect(() => {
    setTagOptions(getTagOptions(tags));
  }, [tags]);

  /**
   * Update the settings and query values based on the new key=value.
   */
  const updateQuery = React.useCallback(
    (key: string, value: any) => {
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
    <>
      <Col gap="0.5rem">
        <h2>{values.name}</h2>
        <Row>
          <Col flex="1">
            <p>An Elasticsearch query provides a way to search for content.</p>
          </Col>
          <Button
            variant={ButtonVariant.secondary}
            onClick={() => {
              setFieldValue('settings', {});
              setFieldValue('query', {});
              setFilter('');
            }}
          >
            Clear Query
          </Button>
        </Row>
        <FormikCheckbox name="settings.searchUnpublished" label="Search unpublished content" />
        <Row alignItems="center">
          <FormikText
            name="settings.size"
            label="Number of Stories"
            type="number"
            width="10ch"
            value={values.settings.size ?? 10}
            onChange={(e) => {
              const value = !!e.target.value ? parseInt(e.target.value) : 10;
              updateQuery('size', value);
            }}
          />
          <p>
            All filters must have a upward limit of content returned in a single request. The
            default is 10, the maximum is 10,000.
          </p>
        </Row>
        <Row>
          <Col>
            <FormikTextArea
              name="settings.search"
              label="Keywords"
              value={values.settings.search ?? ''}
              width={FieldSize.Large}
              rows={8}
              onChange={(e) => {
                const value = e.target.value.length ? e.target.value : undefined;
                updateQuery('search', value);
              }}
            />
          </Col>
          <Col>
            <div>
              The keywords query supports the following operators:
              <ul>
                <li>
                  <code>+</code> signifies AND operation
                </li>
                <li>
                  <code>|</code> signifies OR operation
                </li>
                <li>
                  <code>-</code> negates a single token
                </li>
                <li>
                  <code>"</code> wraps a number of tokens to signify a phrase for searching
                </li>
                <li>
                  <code>*</code> at the end of a term signifies a prefix query
                </li>
                <li>
                  <code>(</code> and <code>)</code> signify precedence
                </li>
                <li>
                  <code>~N</code> after a word signifies edit distance (fuzziness)
                </li>
                <li>
                  <code>~N</code> after a phrase signifies slop amount
                </li>
              </ul>
            </div>
          </Col>
        </Row>
        <Row gap="1rem">
          <label>Search for Keywords in: </label>
          <FormikCheckbox
            name="settings.inHeadline"
            label="Headline"
            onChange={(e) => updateQuery('inHeadline', e.target.checked)}
          />
          <FormikCheckbox
            name="settings.inByline"
            label="Byline"
            onChange={(e) => updateQuery('inByline', e.target.checked)}
          />
          <FormikCheckbox
            name="settings.inStory"
            label="Story text"
            onChange={(e) => updateQuery('inStory', e.target.checked)}
          />
        </Row>
        <Row nowrap>
          <Col>
            <FormikDatePicker
              name="settings.startDate"
              label="Start Date"
              value={values.settings.startDate ?? ''}
              width="13ch"
              onChange={(value) => {
                updateQuery('startDate', value);
              }}
              disabled={!!values.settings.dateOffset}
            />
          </Col>
          <Col>
            <FormikDatePicker
              name="settings.endDate"
              label="End Date"
              value={values.settings.endDate ?? ''}
              width="13ch"
              onChange={(value) => {
                updateQuery('endDate', value);
              }}
              disabled={!!values.settings.dateOffset}
            />
          </Col>
          <Row nowrap>
            <Col justifyContent="center" className="frm-in pad-05">
              <label>Or</label>
            </Col>
            <FormikText
              name="settings.dateOffset"
              label="Date Offset"
              type="number"
              width="10ch"
              value={values.settings.dateOffset ?? ''}
              onChange={(e) => {
                const value = !!e.target.value ? parseInt(e.target.value) : undefined;
                updateQuery('dateOffset', value);
              }}
              disabled={!!values.settings.startDate || !!values.settings.endDate}
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
                  values.settings.actions?.some(
                    (p: { id: number; valueType: string; value: string }) => p.id === mt.value,
                  ),
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
                tagOptions.filter((t) =>
                  values.settings.tags?.some((o: string) => o === t.value),
                ) ?? []
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
      <Col className="code frm-in">
        <label htmlFor="txa-filter">Elasticsearch Query</label>
        <p>
          The query is the expression that is sent to Elasticsearch to find content. Read up on how
          to create a query on the official page{' '}
          <a
            href="https://www.elastic.co/guide/en/elasticsearch/reference/current/search-your-data.html"
            target="_blank"
            rel="noreferrer"
          >
            here
          </a>
          .
        </p>
        <Col className="editor">
          <Editor
            id="txa-filter"
            value={filter}
            onValueChange={(code) => {
              setFilter(code);
              try {
                setFieldValue('query', JSON.parse(code));
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
    </>
  );
};
