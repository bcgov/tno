import { FormikForm } from 'components/formik';
import { ComboBox } from 'features/admin/automation/designer';
import React from 'react';
import { useLookup } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  FormikSelect,
  FormikText,
  FormikTimeInput,
  type ITopicScoreRuleModel,
  Modal,
  OptionItem,
  Row,
  Show,
  useModal,
} from 'tno-core';

import { imageOptions } from './constants';
import { type ITopicScoreRuleForm, type ITopicScoreSourceModel } from './interfaces';
import { toModel } from './utils';
import { TopicScoreRuleSchema } from './validation';

export interface IRuleDrawerProps {
  /** The source the rule belongs to. */
  source: ITopicScoreSourceModel;
  /** The rule being edited; a rule with id 0 is new. */
  rule: ITopicScoreRuleForm;
  /** Sections known for the source. */
  sections: string[];
  /** Save the rule; the drawer closes when it succeeds. */
  onSave: (rule: ITopicScoreRuleModel) => Promise<void>;
  /** Delete the rule. */
  onDelete: (rule: ITopicScoreRuleModel) => Promise<void>;
  /** Close without saving. */
  onClose: () => void;
}

/**
 * Adds or edits one topic score rule. Each rule is saved on its own, immediately.
 * @param param0 Component properties.
 * @returns Component.
 */
export const RuleDrawer: React.FC<IRuleDrawerProps> = ({
  source,
  rule,
  sections,
  onSave,
  onDelete,
  onClose,
}) => {
  const [{ series }] = useLookup();
  const { toggle, isShowing } = useModal();

  // Only the source's own series can be chosen.
  const seriesOptions = [
    new OptionItem('Any', ''),
    ...series
      .filter((s) => s.sourceId === source.id)
      .sort((a, b) => a.name.localeCompare(b.name))
      .map((s) => new OptionItem(s.name, s.id)),
  ];

  return (
    <div className="rule-drawer" role="dialog" aria-label={rule.id ? 'Edit rule' : 'Add rule'}>
      <FormikForm
        initialValues={rule}
        validationSchema={TopicScoreRuleSchema}
        onSubmit={async (values, { setSubmitting }) => {
          try {
            await onSave(toModel(values));
          } catch {
            // Errors are handled globally.
          } finally {
            setSubmitting(false);
          }
        }}
      >
        {({ values, setFieldValue, isSubmitting }) => (
          <Col className="rule-form" gap="0.5rem">
            <h2>
              {rule.id ? 'Edit rule' : 'Add rule'} — {source.name}
            </h2>
            <p className="hint">Leave a condition empty to match anything.</p>
            <FormikSelect
              name="seriesId"
              label="Series"
              options={seriesOptions}
              value={seriesOptions.find((o) => o.value === values.seriesId)}
              onChange={(o) => setFieldValue('seriesId', (o as OptionItem)?.value ?? '')}
            />
            <div className="frm-in">
              <label htmlFor="section">Section</label>
            </div>
            <ComboBox
              name="section"
              aria-label="Section"
              value={values.section}
              suggestions={sections}
              isClearable
              onChange={(value) => setFieldValue('section', value)}
            />
            <Row gap="0.5rem" alignItems="flex-start">
              <FormikText name="pagePrefix" label="Page prefix" width="8ch" maxLength={4} />
              <FormikText name="pageMin" label="Page from" width="8ch" type="number" min={0} />
              <FormikText name="pageMax" label="Page to" width="8ch" type="number" min={0} />
            </Row>
            <FormikSelect
              name="hasImage"
              label="Image"
              width="12ch"
              options={imageOptions}
              value={imageOptions.find((o) => o.value === values.hasImage)}
              onChange={(o) => setFieldValue('hasImage', (o as OptionItem)?.value ?? '')}
            />
            <Row gap="0.5rem" alignItems="flex-start">
              <FormikTimeInput
                name="timeMin"
                label="Time from"
                width="10ch"
                placeholder="HH:MM:SS"
              />
              <FormikTimeInput name="timeMax" label="Time to" width="10ch" placeholder="HH:MM:SS" />
            </Row>
            <Show visible={!!values.timeMin && !!values.timeMax && values.timeMin > values.timeMax}>
              <p className="hint">This range runs overnight, past midnight.</p>
            </Show>
            <Row gap="0.5rem" alignItems="flex-start">
              <FormikText
                name="characterMin"
                label="Characters from"
                width="12ch"
                type="number"
                min={0}
              />
              <FormikText
                name="characterMax"
                label="Characters to"
                width="12ch"
                type="number"
                min={0}
              />
            </Row>
            <FormikText name="score" label="Score" width="8ch" type="number" min={0} required />
            <Row gap="0.5rem" className="drawer-actions">
              <Button type="submit" disabled={isSubmitting}>
                Save
              </Button>
              <Button variant={ButtonVariant.secondary} onClick={onClose} disabled={isSubmitting}>
                Cancel
              </Button>
              <Show visible={!!rule.id}>
                <Button variant={ButtonVariant.danger} onClick={toggle} disabled={isSubmitting}>
                  Delete
                </Button>
              </Show>
            </Row>
          </Col>
        )}
      </FormikForm>
      <Modal
        headerText="Delete rule"
        body="Delete this rule? Stories keep their current score until they are rescored."
        isShowing={isShowing}
        hide={toggle}
        type="delete"
        confirmText="Yes, delete it"
        onConfirm={async () => {
          try {
            await onDelete(toModel(rule));
          } catch {
            // Errors are handled globally.
          } finally {
            toggle();
          }
        }}
      />
    </div>
  );
};
