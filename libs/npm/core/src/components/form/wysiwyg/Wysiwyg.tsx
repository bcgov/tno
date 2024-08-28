import 'react-quill/dist/quill.snow.css';

import { html_beautify } from 'js-beautify';
import { Sources } from 'quill';
import React from 'react';
import ReactQuill from 'react-quill';

import { CustomToolbar } from './CustomToolbar';
import * as styled from './styled';
import { IUrlOption } from './interfaces';
import { FaX } from 'react-icons/fa6';
import { ExpandedWysiwyg } from './ExpandedWysiwyg';
import { useWysiwygContext, WysiwygProvider } from './WysiwygContextProvider';

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

export interface IStateProps {
  html: string;
  text: string;
}

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
  onKeyDown?: (event: React.KeyboardEvent) => void;
  onChange?: (text: string, editor?: any) => void;
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
  const { normalState, setNormalState, expandedState, setExpandedState, expand, setExpand } =
    useWysiwygContext();
  const [showRaw, setShowRaw] = React.useState(false);
  const dialogRef = React.useRef<HTMLDialogElement>(null);

  React.useEffect(() => {
    if (props.value && props.value !== normalState.html) {
      setNormalState({
        ...normalState,
        html: props.value?.replace(/\n+/g, '<br>') ?? '',
      });
    }
    // only update when value changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [props.value]);

  // carry over editor value to raw html or v.v when toggling
  const syncViews = (htmlFromRaw: boolean) => {
    if (htmlFromRaw) setNormalState({ ...normalState, html: normalState.text });
    else setNormalState({ ...normalState, text: normalState.html });
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
      normalState.text
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
    setNormalState({ ...normalState, html: doc.body.textContent });
    props.onChange?.(doc.body.textContent);
  }, [props.onChange, normalState]);

  const onClickFormatRaw = () => {
    const text = html_beautify(normalState.text);
    setNormalState({ ...normalState, text });
    props.onChange?.(text);
  };

  const handleChange = (html: string) => {
    setNormalState({ ...normalState, html: html });
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

  React.useEffect(() => {
    if (expand && !expandedState.html) {
      setExpandedState({ ...normalState });
    }
  }, [expand]);
  return (
    <styled.Wysiwyg viewRaw={showRaw} className={props.className}>
      <WysiwygProvider>
        {props.label && <label className={props.required ? 'required' : ''}>{props.label}</label>}
        <CustomToolbar
          onClickRaw={onClickRaw}
          onChangeContentSelect={onChangeContentSelect}
          urlOptions={props.urlOptions}
          onClickRemoveFormat={stripHtml}
          onClickFormatRaw={onClickFormatRaw}
          onClickClear={() => setNormalState({ ...normalState, html: '', text: '' })}
          onClickExpand={() => {
            setExpand(true);
          }}
          innerRef={setToolBarNode}
        />
        {!!toolBarNode && (
          <>
            <ReactQuill
              className="editor"
              value={normalState.html}
              onChange={handleChange}
              theme="snow"
              modules={modules}
              formats={formats}
              ref={quill}
              onBlur={props.onBlur}
              readOnly={props.disabled}
              onKeyDown={props.onKeyDown}
            />
            <textarea
              id={props.id}
              name={props.name}
              disabled={props.disabled}
              className="raw-editor"
              onChange={(e) => setNormalState({ ...normalState, text: e.target.value })}
              value={normalState.text}
            />
          </>
        )}
        {expand && !!toolBarNode && (
          <dialog id="expand-modal" ref={dialogRef}>
            <ExpandedWysiwyg {...props} />
          </dialog>
        )}
      </WysiwygProvider>
    </styled.Wysiwyg>
  );
};
