import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { useSettings } from 'store/hooks';
import { useNotificationTemplates } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Checkbox,
  Col,
  FormikSelect,
  INotificationModel,
  IOptionItem,
  Overlay,
  Row,
  Show,
} from 'tno-core';

import { defaultNotificationTemplate, defaultRazorTemplate } from './constants';
import { getNotificationTemplateOptions } from './utils';

export const NotificationFormTemplate = () => {
  const { basicAlertTemplateId } = useSettings();
  const { values, setFieldValue } = useFormikContext<INotificationModel>();
  const [{ notificationTemplates }, { findAllNotificationTemplates }] = useNotificationTemplates();

  const [enableEdit, setEnableEdit] = React.useState(false);
  const [templateOptions, setTemplateOptions] = React.useState<IOptionItem[]>(
    getNotificationTemplateOptions(notificationTemplates, values.templateId),
  );

  React.useEffect(() => {
    findAllNotificationTemplates()
      .then((templates) =>
        setTemplateOptions(getNotificationTemplateOptions(templates, values.templateId)),
      )
      .catch(() => {});
    // Fetch users on initial load only.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    setTemplateOptions(getNotificationTemplateOptions(notificationTemplates, values.templateId));
  }, [notificationTemplates, values.templateId]);

  React.useEffect(() => {
    if (values.id === 0 && (!values.templateId || values.templateId === 0)) {
      setFieldValue('templateId', basicAlertTemplateId);
      setFieldValue(
        'template',
        notificationTemplates.find((rt) => rt.id === basicAlertTemplateId),
      );
    }
    // Fetch only when basicAlertTemplateId gets updated.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [basicAlertTemplateId]);

  return (
    <>
      <FormikSelect
        name="templateId"
        label="Template"
        tooltip="A template is used to generate the notification output."
        options={templateOptions}
        value={
          templateOptions.filter(
            (rt) => values.templateId === (rt.value === undefined ? 0 : +rt.value),
          ) ?? ''
        }
        isClearable={false}
        onChange={(newValue) => {
          const option = newValue as IOptionItem;
          const templateId = option.value !== undefined ? +option.value : 0;
          if (templateId) {
            const template = notificationTemplates.find((rt) => rt.id === templateId);
            if (template) {
              setFieldValue('templateId', template.id);
              setFieldValue('template', template);
            }
          } else {
            setFieldValue('templateId', defaultNotificationTemplate.id);
            setFieldValue('template', {
              ...defaultNotificationTemplate,
              name: `${values.name}-${Date.now().toString()}`,
            });
          }
        }}
      />
      <hr />
      <Checkbox
        name="enableEdit"
        label="Enable editing template"
        checked={enableEdit}
        onChange={(e) => setEnableEdit(e.target.checked)}
      />
      <Col className="code">
        <Show visible={!enableEdit}>
          <Overlay />
        </Show>
        <Row>
          <Col flex="1">
            <p>Editing this template will change all notifications that use this template.</p>
          </Col>
          <Button
            variant={ButtonVariant.secondary}
            onClick={() => {
              setFieldValue('template.subject', defaultRazorTemplate.subject);
              setFieldValue('template.body', defaultRazorTemplate.body);
            }}
          >
            Use Default Template
          </Button>
        </Row>
        <Col className="frm-in">
          <label htmlFor="txa-subject">Subject Template</label>
          <Col className="editor">
            <Editor
              id="txa-subject"
              value={values.template?.subject ?? ''}
              onValueChange={(code) => setFieldValue('template.subject', code)}
              highlight={(code) => {
                return highlight(code, languages.cshtml, 'razor');
              }}
            />
          </Col>
        </Col>
        <Col className="frm-in">
          <label htmlFor="txa-template">Body Template</label>
          <Col className="editor">
            <Editor
              id="txa-template"
              value={values.template?.body ?? ''}
              onValueChange={(code) => setFieldValue('template.body', code)}
              highlight={(code) => {
                return highlight(code, languages.cshtml, 'razor');
              }}
            />
          </Col>
        </Col>
      </Col>
    </>
  );
};
