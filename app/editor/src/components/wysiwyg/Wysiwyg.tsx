import 'react-quill/dist/quill.snow.css';

import { useFormikContext } from 'formik';
import { html_beautify } from 'js-beautify';
import { Sources } from 'quill';
import React from 'react';
import ReactQuill from 'react-quill';
import { useParams } from 'react-router-dom';
import { useLookup } from 'store/hooks';
import { Error } from 'tno-core';

import { CustomToolbar } from './CustomToolbar';
import * as styled from './styled';

const FORMATS = [
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
export interface IWysiwygProps<T extends object> {
  /** The element class name */
  className?: string;
  /** the formik field that is being used within the WYSIWYG */
  fieldName: keyof T;
  /** optional label to appear above the WYSIWYG */
  label?: string;
  /** whether or not it is a required field */
  required?: boolean;
  /** Whether to show the raw view. */
  viewRaw?: boolean;
  /** Whether the text area has a custom height. */
  height?: string;
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
  className,
  fieldName,
  label,
  required,
  expandModal,
  height,
  onBlur,
}: IWysiwygProps<T>) => {
  const { values, setFieldValue, errors, touched } = useFormikContext<T>();
  const [toolBarNode, setToolBarNode] = React.useState();

  const { id } = useParams();
  const [{ tags }] = useLookup();

  const [state, setState] = React.useState({
    html: '',
    rawHtml: '',
  });
  const [showRaw, setShowRaw] = React.useState(false);

  React.useEffect(() => {
    const html = (values[fieldName] as string)?.replace(/\n+/g, '<br>') ?? '';
    if (html !== state.html && (!height || !state.html)) {
      setState({
        ...state,
        html: html,
      });
      updateTags(html);
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
    const doc = new DOMParser().parseFromString(
      (values[fieldName] as string)
        .replace(/<p\s*[^>]*>/g, '[p]')
        .replaceAll('</p>', '[/p]')
        .replace(/<br\s*\/?>/g, '[br]'),
      'text/html',
    );
    const html =
      doc.body.textContent
        ?.replaceAll('[p]', '<p>')
        .replaceAll('[/p]', '</p>')
        .replaceAll('[br]', '<br>') || '';
    if (html !== state.html) {
      setState({ ...state, html: html });
      setFieldValue(fieldName as string, html, true);
    }
  };

  const extractTags = (values: string[]) => {
    return tags.filter((tag) =>
      values.some((value: string) => value.trim().toLowerCase() === tag.code.toLowerCase()),
    );
  };

  const updateTags = (html: string) => {
    const stringValue = html.match(/\[([^\]]+)\][^[\]]*$/s)?.pop();
    const tagValues = stringValue?.includes(',')
      ? stringValue?.split(',')
      : stringValue?.split(' ') ?? [];
    const currentTags = (values as any)?.tags;
    const newTags = extractTags(tagValues).filter((item) => !currentTags.includes(item));
    if (newTags.length > 0) setFieldValue('tags', [...currentTags, ...newTags], true);
  };

  const handleChange = (html: string) => {
    if (html !== state.html) {
      setState({ ...state, html: html });
      const value = html === '<p><br></p>' ? '' : html;
      setFieldValue(fieldName as string, value, true);
      updateTags(html);
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

  return (
    <styled.Wysiwyg viewRaw={showRaw} height={height} className={className} fieldName={fieldName}>
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
            formats={FORMATS}
            onBlur={onBlur}
          />
          <textarea
            className="raw-editor"
            onChange={(e) => setState({ ...state, rawHtml: e.target.value })}
            value={state.rawHtml}
          />
        </>
      )}
      <Error error={touched[fieldName] ? (errors[fieldName] as string) : ''} />
    </styled.Wysiwyg>
  );
};
