import { FormikForm } from 'components/formik';
import React from 'react';
import { FaArrowDown, FaArrowUp, FaTrash } from 'react-icons/fa';
import { useLookup, useLookupOptions } from 'store/hooks';
import { useTopicScoreRules } from 'store/hooks/admin';
import { IAjaxRequest } from 'store/slices';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  FormikTimeInput,
  FormPage,
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
export const TopicScoreRuleList: React.FC = () => {
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
          {({ values, setFieldValue }) => (
            <>
              <Row className="add-media">
                <h2>Scoring by source</h2>
              </Row>
              <Row className="row-header">
                <Col className="f3">Source</Col>
                <Col className="f3">Program/Show</Col>
                <Col className="f3">Section</Col>
                <Col>Page min</Col>
                <Col>Page max</Col>
                <Col>Has Image</Col>
                <Col className="f2">Text min</Col>
                <Col className="f2">Text max</Col>
                <Col className="f2">Time min</Col>
                <Col className="f2">Time max</Col>
                <Col>Score</Col>
                <Col>&nbsp;</Col>
              </Row>
              {values.map((item, index) => (
                <Show key={`${item}-${index}`} visible={!item.remove}>
                  <Show visible={index === 0 && !!rules.length}>
                    <Row className="add-row" onClick={() => addTopicScoreRule(index, values)}></Row>
                  </Show>
                  <Row className={`row${item.id === 0 ? ' adding' : ''}`}>
                    <Col
                      className="f3"
                      data-tooltip-content={
                        sourceOptions.find((o) => o.value === item.sourceId)?.label ?? ''
                      }
                      data-tooltip-id="main-tooltip"
                    >
                      <FormikSelect
                        name={`${index}.sourceId`}
                        options={sourceOptions}
                        value={sourceOptions.find((o) => o.value === item.sourceId) ?? ''}
                        width={FieldSize.Small}
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
                    </Col>
                    <Col
                      className="f3"
                      data-tooltip-content={
                        seriesOptions.find((o) => o.value === item.seriesId)?.label ?? ''
                      }
                      data-tooltip-id="main-tooltip"
                    >
                      <FormikSelect
                        name={`${index}.seriesId`}
                        options={getSeriesOptions(values[index].sourceId)}
                        value={seriesOptions.find((o) => o.value === item.seriesId) ?? ''}
                        width={FieldSize.Small}
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
                    </Col>
                    <Col className="f3">
                      <FormikText name={`${index}.section`} width={FieldSize.Small} />
                    </Col>
                    <Col>
                      <FormikText name={`${index}.pageMin`} width="3em" />
                    </Col>
                    <Col>
                      <FormikText name={`${index}.pageMax`} width="3em" />
                    </Col>
                    <Col>
                      <FormikCheckbox name={`${index}.hasImage`} value={true} />
                    </Col>
                    <Col>
                      <FormikText name={`${index}.characterMin`} className="f2" width="4em" />
                    </Col>
                    <Col>
                      <FormikText name={`${index}.characterMax`} className="f2" width="4em" />
                    </Col>
                    <Col className="f2">
                      <FormikTimeInput
                        name={`${index}.timeMin`}
                        width="7em"
                        value={!!item.timeMin ? item.timeMin : ''}
                        placeholder={!!item.timeMin ? item.timeMin : 'HH:MM:SS'}
                      />
                    </Col>
                    <Col className="f2">
                      <FormikTimeInput
                        name={`${index}.timeMax`}
                        width="7em"
                        value={!!item.timeMax ? item.timeMax : ''}
                        placeholder={!!item.timeMax ? item.timeMax : 'HH:MM:SS'}
                      />
                    </Col>
                    <Col>
                      <FormikText name={`${index}.score`} width="3.5em" />
                    </Col>
                    <Col alignContent="flex-end" className="actions">
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
                    </Col>
                  </Row>
                  <Show visible={!!rules.length}>
                    <Row
                      className="add-row"
                      onClick={() => addTopicScoreRule(index + 1, values)}
                    ></Row>
                  </Show>
                </Show>
              ))}
              <Row className="form-footer" justifyContent="flex-end">
                <Button type="submit" variant={ButtonVariant.primary}>
                  Save Changes
                </Button>
              </Row>
            </>
          )}
        </FormikForm>
      </FormPage>
    </styled.TopicScoreRuleList>
  );
};
