import 'react-quill/dist/quill.snow.css';

import { html_beautify } from 'js-beautify';
import { Sources } from 'quill';
import React from 'react';
import ReactQuill from 'react-quill';

import { CustomToolbar } from './CustomToolbar';
import * as styled from './styled';
import { IUrlOption } from './interfaces';
import { FaX } from 'react-icons/fa6';

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

export interface IWysiwygProps {
  /** Input id attribute. */
  id?: string;
  /** the field name that is being used within the WYSIWYG */
  name?: string;
  /** optional label to appear above the WYSIWYG */
  label?: string;
  /** options to choose from a dropdown that will hyperlink the url attribute */
  urlOptions?: { label: string; url: string; section: string }[];
  /** Whether component is disabled. */
  disabled?: boolean;
  /** whether or not it is a required field */
  required?: boolean;
  /** The value */
  value?: string;
  /** Whether to enable raw view */
  viewRaw?: boolean;
  /** Default height of component. */
  height?: string;
  /** className */
  className?: string;
  onChange?: (text: string) => void;
  onBlur?: (
    previousSelection: ReactQuill.Range,
    source: Sources,
    editor: ReactQuill.UnprivilegedEditor,
  ) => void;
}
/**
 * A WYSIWYG component.
 * @param props Component props.
 * @returns A component.
 */
export const Wysiwyg: React.FC<IWysiwygProps> = (props) => {
  const [toolBarNode, setToolBarNode] = React.useState();

  const quill = React.useRef<ReactQuill>(null);
  const [state, setState] = React.useState({
    html: '',
    text: props.value ?? '',
  });
  const [showRaw, setShowRaw] = React.useState(false);
  const [expand, setExpand] = React.useState(false);
  const dialogRef = React.useRef<HTMLDialogElement>(null);

  React.useEffect(() => {
    setState({
      ...state,
      html: props.value?.replace(/\n+/g, '<br>') ?? '',
    });
    // Only update when the value changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [props.value]);

  // carry over editor value to raw html or v.v when toggling
  const syncViews = (htmlFromRaw: boolean) => {
    if (htmlFromRaw) setState({ ...state, html: state.text });
    else setState({ ...state, text: state.html });
  };

  // toggle raw html view
  const onClickRaw = () => {
    const fromRawHtml = showRaw;
    setShowRaw(!showRaw);
    syncViews(fromRawHtml);
  };

  const stripHtml = React.useCallback(() => {
    // strip html from string
    const doc = new DOMParser().parseFromString(
      state.text
        .replace(/<p\s*[^>]*>/g, '[p]')
        .replaceAll('</p>', '[/p]')
        .replace(/<br\s*\/?>/g, '[br]'),
      'text/html',
    );
    doc.body.textContent =
      doc.body.textContent
        ?.replaceAll('[p]', '<p>')
        .replaceAll('[/p]', '</p>')
        .replaceAll('[br]', '<br>') || '';
    setState({ ...state, html: doc.body.textContent });
    props.onChange?.(doc.body.textContent);
  }, [props.onChange, state]);

  const onClickFormatRaw = () => {
    const text = html_beautify(state.text);
    setState({ ...state, text });
    props.onChange?.(text);
  };

  const handleChange = (html: string) => {
    setState({ ...state, html: html });
    if (html === '<p><br></p>') {
      props.onChange?.('');
    } else {
      props.onChange?.(html);
    }
  };

  const modules = React.useMemo(() => {
    // toolbar configuration
    // https://quilljs.com/docs/modules/toolbar/
    // why we have matchVisual: false:
    // it will prevent the editor reate clicked bullet list button in full screen modal.
    // There is a bug in quilljs 2.0, in this situation, it will create a new line after refresh,
    // and it will inpact useEffect related to value change, and it causes infinite loop to create a new line.
    const config = {
      toolbar: {
        container: toolBarNode,
      },
      clipboard: {
        matchVisual: false,
      },
    };

    return config;
  }, [toolBarNode]);

  React.useEffect(() => {
    if (expand) {
      dialogRef.current?.showModal();
    } else {
      dialogRef.current?.close();
    }
  }, [expand]);

  const onChangeContentSelect = (value: IUrlOption) => {
    if (!quill.current) return;
    try {
      const editor = quill.current.getEditor();
      // put tag at current cursor position or at the end of the text
      const selection = editor.getSelection();
      const index = selection ? selection.index : editor.getLength() - 1;

      editor.insertText(index, value.label, 'link', value.url);
    } catch (error) {
      console.error('Failed to insert text:', error);
    }
  };
  return (
    <styled.Wysiwyg viewRaw={showRaw} className={props.className}>
      {props.label && <label className={props.required ? 'required' : ''}>{props.label}</label>}
      <CustomToolbar
        onClickRaw={onClickRaw}
        onChangeContentSelect={onChangeContentSelect}
        urlOptions={props.urlOptions}
        onClickRemoveFormat={stripHtml}
        onClickFormatRaw={onClickFormatRaw}
        onClickClear={() => setState({ ...state, html: '', text: '' })}
        onClickExpand={() => setExpand(true)}
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
            ref={quill}
            onBlur={props.onBlur}
            readOnly={props.disabled}
          />
          <textarea
            id={props.id}
            name={props.name}
            disabled={props.disabled}
            className="raw-editor"
            onChange={(e) => setState({ ...state, text: e.target.value })}
            value={state.text}
          />
        </>
      )}
      {expand && !!toolBarNode && (
        <dialog id="expand-modal" ref={dialogRef}>
          <FaX className="exit" onClick={() => setExpand(false)} />
          <Wysiwyg {...props} />
        </dialog>
      )}
    </styled.Wysiwyg>
  );
};
