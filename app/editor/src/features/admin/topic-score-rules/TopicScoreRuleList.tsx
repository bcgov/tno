import { FormikForm } from 'components/formik';
import { FormPage } from 'components/formpage';
import React from 'react';
import { FaArrowDown, FaArrowUp, FaTrash } from 'react-icons/fa';
import { useLookup, useLookupOptions } from 'store/hooks';
import { useTopicScoreRules } from 'store/hooks/admin';
import { IAjaxRequest } from 'store/slices';
import {
  Button,
  ButtonVariant,
  Col,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  FormikTimeInput,
  getSortableOptions,
  IOptionItem,
  Loader,
  Row,
  Show,
} from 'tno-core';

import { defaultTopicScoreRule } from './constants';
import { ITopicScoreRuleForm } from './interfaces';
import * as styled from './styled';
import { toForm, toModel } from './utils';
import { TopicScoreRulesSchema } from './validation';

/**
 * Component to display a list of rules.
 * Provides CRUD methods for rules.
 * @returns Component
 */
const TopicScoreRuleList: React.FC = () => {
  const [{ rules }, api] = useTopicScoreRules();
  const [{ series }] = useLookup();
  const [{ sourceOptions, seriesOptions }] = useLookupOptions();

  const [items, setItems] = React.useState<ITopicScoreRuleForm[]>([]);
  const [isSaving, setIsSaving] = React.useState(false);

  const findAll = React.useCallback(async () => {
    await api.findAllTopicScoreRules().then((data) => {
      if (!!data.length) setItems(data.map((r) => toForm(r)));
      else setItems([defaultTopicScoreRule]);
    });
  }, [api]);

  React.useEffect(() => {
    findAll();
    // Only load the topic score rules the first time.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Filter out series that don't belong to the selected source.
  const getSeriesOptions = React.useCallback(
    (sourceId: number | string) => {
      const id = +sourceId;
      return getSortableOptions(
        series.filter((s) => s.sourceId === undefined || s.sourceId === id),
      );
    },
    [series],
  );

  const handleSave = async (values: ITopicScoreRuleForm[]) => {
    if (!isSaving) {
      try {
        setIsSaving(true);
        const results = await api.updateTopicScoreRules(values.map((r) => toModel(r)));
        setItems(results.map((r) => toForm(r)));
      } catch {
        // Errors are handled globally,
      } finally {
        setIsSaving(false);
      }
    }
  };

  const addTopicScoreRule = (index: number, values: ITopicScoreRuleForm[]) => {
    var items = [...values];
    var newItem = { ...defaultTopicScoreRule, sortOrder: index };
    if (
      index > 0 &&
      index < values.length &&
      values[index - 1].sourceId === values[index].sourceId
    ) {
      newItem.sourceId = values[index].sourceId;
    }
    items.splice(index, 0, newItem);
    setItems(items);
  };

  return (
    <styled.TopicScoreRuleList>
      <FormPage>
        <Loader visible={isSaving}></Loader>
        <FormikForm
          onSubmit={handleSave}
          initialValues={items}
          validationSchema={TopicScoreRulesSchema}
          loading={(request: IAjaxRequest) =>
            !request.isSilent && request.group.some((g) => g === 'content' || g === 'lookup')
          }
        >
          {({ values, setFieldValue, isSubmitting }) => (
            <div>
              <Row className="add-media">
                <h2>Scoring by source</h2>
              </Row>
              <div className="grid-table">
                <div>Source</div>
                <div>Show/Program</div>
                <div>Section</div>
                <div className="center">
                  Page
                  <br />
                  Min
                </div>
                <div className="center">
                  Page
                  <br />
                  Max
                </div>
                <div className="center">
                  Has
                  <br />
                  Image
                </div>
                <div className="center">
                  Text
                  <br />
                  Min
                </div>
                <div className="center">
                  Text
                  <br />
                  Max
                </div>
                <div>Time Min</div>
                <div>Time Max</div>
                <div>Score</div>
                <div>&nbsp;</div>
                {values.map((item, index) => {
                  const optionSource = sourceOptions.find((o) => o.value === item.sourceId);
                  const tooltipSource =
                    optionSource && typeof optionSource.label === 'string'
                      ? optionSource.label ?? ''
                      : '';

                  const optionSeries = seriesOptions.find((o) => o.value === item.seriesId);
                  const tooltipSeries =
                    optionSeries && typeof optionSeries.label === 'string'
                      ? optionSeries.label ?? ''
                      : '';

                  return (
                    <Show key={`${item}-${index}`} visible={!item.remove}>
                      <Show visible={index === 0 && !!rules.length}>
                        <Row
                          className="add-row"
                          onClick={() => addTopicScoreRule(index, values)}
                        ></Row>
                      </Show>
                      <div className={`grid-row${item.id === 0 ? ' adding' : ''}`}>
                        <div data-tooltip-content={tooltipSource} data-tooltip-id="main-tooltip">
                          <FormikSelect
                            name={`${index}.sourceId`}
                            required
                            options={sourceOptions}
                            value={sourceOptions.find((o) => o.value === item.sourceId) ?? ''}
                            isClearable={false}
                            menuPortalTarget={document.body}
                            styles={{ menuPortal: (base) => ({ ...base, zIndex: 2 }) }}
                            onChange={(newValue) => {
                              const value = (newValue as IOptionItem).value;
                              if (!!value) {
                                setItems(
                                  values.map((t, i) =>
                                    i === index ? { ...item, sourceId: value as number } : t,
                                  ),
                                );
                              }
                            }}
                          />
                        </div>
                        <div data-tooltip-content={tooltipSeries} data-tooltip-id="main-tooltip">
                          <FormikSelect
                            name={`${index}.seriesId`}
                            options={getSeriesOptions(values[index].sourceId)}
                            value={seriesOptions.find((o) => o.value === item.seriesId) ?? ''}
                            menuPortalTarget={document.body}
                            styles={{ menuPortal: (base) => ({ ...base, zIndex: 2 }) }}
                            clearValue=""
                            isDisabled={!values[index].sourceId}
                            onChange={(newValue) => {
                              const value = (newValue as IOptionItem).value;
                              if (!!value) {
                                setItems(
                                  values.map((t, i) =>
                                    i === index ? { ...item, seriesId: value as number } : t,
                                  ),
                                );
                              }
                            }}
                          />
                        </div>
                        <div>
                          <FormikText name={`${index}.section`} />
                        </div>
                        <div className={`center`}>
                          <FormikText
                            name={`${index}.pageMin`}
                            width="4em"
                            className="text-right"
                          />
                        </div>
                        <div className={`center`}>
                          <FormikText
                            name={`${index}.pageMax`}
                            width="4em"
                            className="text-right"
                          />
                        </div>
                        <div className={`center`}>
                          <FormikCheckbox name={`${index}.hasImage`} value={true} />
                        </div>
                        <div className={`center`}>
                          <FormikText
                            name={`${index}.characterMin`}
                            width="4em"
                            className="text-right"
                          />
                        </div>
                        <div className={`center`}>
                          <FormikText
                            name={`${index}.characterMax`}
                            width="4em"
                            className="text-right"
                          />
                        </div>
                        <div>
                          <FormikTimeInput
                            name={`${index}.timeMin`}
                            width="7em"
                            value={!!item.timeMin ? item.timeMin : ''}
                            placeholder={!!item.timeMin ? item.timeMin : 'HH:MM:SS'}
                          />
                        </div>
                        <div>
                          <FormikTimeInput
                            name={`${index}.timeMax`}
                            width="7em"
                            value={!!item.timeMax ? item.timeMax : ''}
                            placeholder={!!item.timeMax ? item.timeMax : 'HH:MM:SS'}
                          />
                        </div>
                        <div>
                          <FormikText
                            name={`${index}.score`}
                            required
                            width="3.5em"
                            className="text-right"
                          />
                        </div>
                        <div className={`actions`}>
                          <Row nowrap>
                            <Col>
                              <Button
                                variant={ButtonVariant.link}
                                className="move"
                                disabled={
                                  index < 1 ||
                                  items.length < index ||
                                  items[index - 1].sourceId !== item.sourceId
                                }
                                onClick={() => {
                                  var results = [...values];
                                  var above = results[index - 1];
                                  results.splice(index, 1);
                                  results.splice(index - 1, 0, {
                                    ...item,
                                    sortOrder: above.sortOrder,
                                  });
                                  setItems(results.map((r, i) => ({ ...r, sortOrder: i })));
                                }}
                              >
                                <FaArrowUp />
                              </Button>
                              <Button
                                variant={ButtonVariant.link}
                                className="move"
                                disabled={
                                  index >= items.length - 1 ||
                                  items[index + 1].sourceId !== item.sourceId
                                }
                                onClick={async () => {
                                  var results = [...values];
                                  var below = results[index + 1];
                                  results.splice(index, 1);
                                  results.splice(index + 1, 0, {
                                    ...item,
                                    sortOrder: below.sortOrder,
                                  });
                                  below.sortOrder--;
                                  setItems(results.map((r, i) => ({ ...r, sortOrder: i })));
                                }}
                              >
                                <FaArrowDown />
                              </Button>
                            </Col>
                            <Button
                              variant={ButtonVariant.danger}
                              disabled={!rules.length}
                              onClick={async (e) => {
                                setFieldValue(`${index}.remove`, true);
                              }}
                            >
                              <FaTrash />
                            </Button>
                          </Row>
                        </div>
                      </div>
                      <Show visible={!!rules.length}>
                        <Row
                          className="add-row"
                          onClick={() => addTopicScoreRule(index + 1, values)}
                        ></Row>
                      </Show>
                    </Show>
                  );
                })}
              </div>
              <Row className="form-footer" justifyContent="flex-end">
                <Button type="submit" variant={ButtonVariant.primary} disabled={isSubmitting}>
                  Save Changes
                </Button>
              </Row>
            </div>
          )}
        </FormikForm>
      </FormPage>
    </styled.TopicScoreRuleList>
  );
};

export default TopicScoreRuleList;
