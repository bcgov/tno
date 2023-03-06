import { FormikForm } from 'components/formik';
import { useLookupOptions } from 'hooks';
import React from 'react';
import { FaArrowDown, FaArrowUp, FaTrash } from 'react-icons/fa';
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
  IOptionItem,
  Loader,
  Row,
  Show,
} from 'tno-core';

import { defaultTopicScoreRule } from './constants';
import { ITopicScoreRuleForm } from './interfaces';
import * as styled from './styled';
import { toForm, toModel } from './utils';

/**
 * Component to display a list of rules.
 * Provides CRUD methods for rules.
 * @returns Component
 */
export const TopicScoreRuleList: React.FC = () => {
  const [{ rules }, api] = useTopicScoreRules();
  const [{ sourceOptions }] = useLookupOptions();

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

  const addTopicScoreRule = (index: number) => {
    var values = [...items];
    values.splice(index, 0, { ...defaultTopicScoreRule, sortOrder: index });
    setItems(values);
  };

  return (
    <styled.TopicScoreRuleList>
      <FormPage>
        <Loader visible={isSaving}></Loader>
        <FormikForm
          onSubmit={(values) => handleSave(values)}
          initialValues={items}
          loading={(request: IAjaxRequest) =>
            !request.isSilent && request.group.some((g) => g === 'content' || g === 'lookup')
          }
        >
          {({ values, setFieldValue }) => (
            <>
              <Row className="add-media">
                <h2>Scoring by source</h2>
              </Row>
              <Col>
                <Row className="row-header">
                  <Col flex="2 2 0">Source</Col>
                  <Col flex="2 2 0">Section</Col>
                  <Col flex="1 1 0">Page min</Col>
                  <Col flex="1 1 0">Page max</Col>
                  <Col flex="1 1 0">Image</Col>
                  <Col flex="1 1 0">Char min</Col>
                  <Col flex="1 1 0">Char max</Col>
                  <Col flex="1 1 0">Time min</Col>
                  <Col flex="1 1 0">Time max</Col>
                  <Col flex="1 1 0">Score</Col>
                  <Col flex="1 1 0">&nbsp;</Col>
                </Row>
                <Col className="rows">
                  {values.map((item, index) => (
                    <Show key={`${item}-${index}`} visible={!item.remove}>
                      <Show visible={index === 0 && !!rules.length}>
                        <Row className="add-row" onClick={() => addTopicScoreRule(index)}></Row>
                      </Show>
                      <Row className={`row${item.id === 0 ? ' adding' : ''}`}>
                        <Col
                          flex="2 2 0"
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
                                  items.map((t, i) =>
                                    i === index ? { ...item, sourceId: value as number } : t,
                                  ),
                                );
                              }
                            }}
                          />
                        </Col>
                        <Col flex="2 2 0">
                          <FormikText name={`${index}.section`} width={FieldSize.Small} />
                        </Col>
                        <Col flex="1 1 0">
                          <FormikText name={`${index}.pageMin`} width={FieldSize.Tiny} />
                        </Col>
                        <Col flex="1 1 0">
                          <FormikText name={`${index}.pageMax`} width={FieldSize.Tiny} />
                        </Col>
                        <Col flex="1 1 0">
                          <FormikCheckbox name={`${index}.hasImage`} value={true} />
                        </Col>
                        <Col flex="1 1 0">
                          <FormikText name={`${index}.characterMin`} width={FieldSize.Tiny} />
                        </Col>
                        <Col flex="1 1 0">
                          <FormikText name={`${index}.characterMax`} width={FieldSize.Tiny} />
                        </Col>
                        <Col flex="1 1 0">
                          <FormikTimeInput
                            name={`${index}.timeMin`}
                            width="7em"
                            value={!!item.timeMin ? item.timeMin : ''}
                            placeholder={!!item.timeMin ? item.timeMin : 'HH:MM:SS'}
                          />
                        </Col>
                        <Col flex="1 1 0">
                          <FormikTimeInput
                            name={`${index}.timeMax`}
                            width="7em"
                            value={!!item.timeMax ? item.timeMax : ''}
                            placeholder={!!item.timeMax ? item.timeMax : 'HH:MM:SS'}
                          />
                        </Col>
                        <Col flex="1 1 0">
                          <FormikText name={`${index}.score`} width={FieldSize.Tiny} />
                        </Col>
                        <Col flex="1 1 0" alignContent="flex-end" className="actions">
                          <Row>
                            <Show visible={!!item.id}>
                              <Col>
                                <Button
                                  variant={ButtonVariant.link}
                                  className="move"
                                  disabled={
                                    index < 1 || items[index - 1].sourceId !== item.sourceId
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
                            </Show>
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
                        <Row className="add-row" onClick={() => addTopicScoreRule(index + 1)}></Row>
                      </Show>
                    </Show>
                  ))}
                </Col>
                <Row className="form-footer" justifyContent="flex-end">
                  <Button type="submit" variant={ButtonVariant.primary}>
                    Save Changes
                  </Button>
                </Row>
              </Col>
            </>
          )}
        </FormikForm>
      </FormPage>
    </styled.TopicScoreRuleList>
  );
};
