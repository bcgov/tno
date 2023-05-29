import 'react-quill/dist/quill.snow.css';

import { useFormikContext } from 'formik';
import { html_beautify } from 'js-beautify';
import { Sources } from 'quill';
import React from 'react';
import ReactQuill from 'react-quill';
import { useParams } from 'react-router-dom';
import { Error } from 'tno-core';

import { CustomToolbar } from './CustomToolbar';
import * as styled from './styled';

export interface IWysiwygProps<T extends object> {
  /** the formik field that is being used within the WYSIWYG */
  fieldName?: keyof T;
  /** optional label to appear above the WYSIWYG */
  label?: string;
  /** whether or not it is a required field */
  required?: boolean;
  /** Whether to show the raw view. */
  viewRaw?: boolean;
  /** Whether the text area has a height. */
  hasHeight?: boolean;
  /** Event fires when modal is expanded. */
  expandModal?: (show: boolean) => void;
  /** onBlur event. */
  onBlur?: (
    previousSelection: ReactQuill.Range,
    source: Sources,
    editor: ReactQuill.UnprivilegedEditor,
  ) => void;
}
/**
 * A WYSIWYG editor for the content summary form
 * @param fieldName The name of the field to edit, MUST BE of type string.
 * @param label The label to display above the editor
 * @param required Whether or not the field is required
 * @returns A WYSIWYG editor for the content summary form
 */
export const Wysiwyg = <T extends object>({
  fieldName,
  label,
  required,
  expandModal,
  hasHeight,
  onBlur,
}: IWysiwygProps<T>) => {
  const { values, setFieldValue, errors, touched } = useFormikContext<T>();
  const [toolBarNode, setToolBarNode] = React.useState();

  const { id } = useParams();

  const [state, setState] = React.useState({
    html: '',
    rawHtml: '',
  });
  const [showRaw, setShowRaw] = React.useState(false);

  React.useEffect(() => {
    if (!!fieldName) {
      setState({
        ...state,
        html: (values[fieldName] as string)?.replace(/\n/g, '<br />') ?? '',
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, values, fieldName]);

  // carry over editor value to raw html or v.v when toggling
  const syncViews = (htmlFromRaw: boolean) => {
    if (htmlFromRaw) setState({ ...state, html: state.rawHtml });
    else setState({ ...state, rawHtml: state.html });
  };

  // toggle raw html view
  const onClickRaw = () => {
    const fromRawHtml = showRaw;
    setShowRaw(!showRaw);
    syncViews(fromRawHtml);
  };

  const onClickFormatRaw = () => {
    setState({ ...state, rawHtml: html_beautify(state.rawHtml) });
  };

  const stripHtml = () => {
    // strip html from string
    if (!!fieldName) {
      let doc = new DOMParser().parseFromString((values[fieldName] as string) ?? '', 'text/html');
      setFieldValue(fieldName as string, doc.body.textContent || '');
      setState({ ...state, html: doc.body.textContent || '' });
    }
  };

  const handleChange = (html: string) => {
    setState({ ...state, html: html });
    if (!!fieldName) {
      setFieldValue(fieldName as string, html);
      if (html === '<p><br></p>') {
        setFieldValue(fieldName as string, '');
      }
    }
  };

  const modules = React.useMemo(() => {
    const config = {
      toolbar: {
        container: toolBarNode,
      },
    };

    return config;
  }, [toolBarNode]);

  const formats = [
    'header',
    'font',
    'size',
    'bold',
    'italic',
    'underline',
    'strike',
    'blockquote',
    'list',
    'bullet',
    'indent',
    'link',
    'image',
    'color',
  ];

  return (
    <styled.Wysiwyg viewRaw={showRaw} hasHeight={hasHeight}>
      {label && <label className={required ? 'required' : ''}>{label}</label>}
      <CustomToolbar
        onClickRaw={onClickRaw}
        onClickRemoveFormat={stripHtml}
        onClickFormatRaw={onClickFormatRaw}
        onClickClear={() => setState({ ...state, html: '', rawHtml: '' })}
        onClickExpand={() => {
          if (expandModal) expandModal(true);
        }}
        innerRef={setToolBarNode}
      />
      {!!toolBarNode && (
        <>
          <ReactQuill
            className="editor"
            value={state.html}
            onChange={handleChange}
            theme="snow"
            modules={modules}
            formats={formats}
            onBlur={onBlur}
          />
          <textarea
            className="raw-editor"
            onChange={(e) => setState({ ...state, rawHtml: e.target.value })}
            value={state.rawHtml}
          />
        </>
      )}
      <Error error={!!fieldName && touched[fieldName] ? (errors[fieldName] as string) : ''} />
    </styled.Wysiwyg>
  );
};
