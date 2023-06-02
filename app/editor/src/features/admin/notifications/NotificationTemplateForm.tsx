import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import Editor from 'react-simple-code-editor';
import { Col, INotificationModel } from 'tno-core';

export const NotificationTemplateForm = () => {
  const { values, setFieldValue } = useFormikContext<INotificationModel>();

  return (
    <div>
      <Col className="code frm-in">
        <label htmlFor="txa-subject">Subject Template</label>
        <Col className="editor">
          <Editor
            id="txa-subject"
            value={values.settings.subject ?? ''}
            onValueChange={(code) => setFieldValue('settings.subject', code)}
            highlight={(code) => {
              return highlight(code, languages.cshtml, 'razor');
            }}
          />
        </Col>
      </Col>
      <Col className="code frm-in">
        <label htmlFor="txa-template">Notification Template</label>
        <Col className="editor">
          <Editor
            id="txa-template"
            value={values.template}
            onValueChange={(code) => setFieldValue('template', code)}
            highlight={(code) => {
              return highlight(code, languages.cshtml, 'razor');
            }}
          />
        </Col>
      </Col>
    </div>
  );
};
