import 'react-quill/dist/quill.snow.css';

import { useFormikContext } from 'formik';
import { IContentModel } from 'hooks';
import { html_beautify } from 'js-beautify';
import _ from 'lodash';
import React from 'react';
import ReactQuill from 'react-quill';
import { useParams } from 'react-router-dom';
import { useLookup } from 'store/hooks';

import { Error } from '../error';
import { CustomToolbar } from './CustomToolbar';
import * as styled from './styled';

export interface IWysiwygProps {
  /** the formik field that is being used within the WYSIWYG */
  fieldName?: keyof IContentModel;
  /** optional label to appear above the WYSIWYG */
  label?: string;
  /** whether or not it is a required field */
  required?: boolean;
  expandModal?: (show: boolean) => void;
  viewRaw?: boolean;
  hasHeight?: boolean;
}
/**
 * A WYSIWYG editor for the content summary form
 * @param fieldName The name of the field to edit, MUST BE of type string.
 * @param label The label to display above the editor
 * @param required Whether or not the field is required
 * @returns A WYSIWYG editor for the content summary form
 */
export const Wysiwyg: React.FC<IWysiwygProps> = ({
  fieldName,
  label,
  required,
  expandModal,
  hasHeight,
}) => {
  const { values, setFieldValue, errors, touched } = useFormikContext<IContentModel>();
  const [toolBarNode, setToolBarNode] = React.useState();
  const [{ tags }] = useLookup();

  const { id } = useParams();

  const [state, setState] = React.useState({
    html: '',
    rawHtml: '',
  });
  const [showRaw, setShowRaw] = React.useState(false);

  React.useEffect(() => {
    if (!!id) {
      setState({ ...state, html: values[fieldName!] as string });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, values, fieldName]);

  const tagMatch = /\[.*?\]/g;

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

  const extractTags = (values: string[]) => {
    return tags
      .filter((tag) => values.some((value: string) => value.toLowerCase() === tag.id.toLowerCase()))
      .map((tag) => tag);
  };

  const stripHtml = () => {
    // strip html from string
    let doc = new DOMParser().parseFromString(values[fieldName!] as string, 'text/html');
    setFieldValue(fieldName!, doc.body.textContent || '');
    setState({ ...state, html: doc.body.textContent || '' });
  };

  const handleChange = (html: string) => {
    setState({ ...state, html: html });
    setFieldValue(fieldName!, html);
    if (html === '<p><br></p>') {
      setFieldValue(fieldName!, '');
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
            onBlur={() => {
              const value = values[fieldName!];
              if (!!value && typeof value === 'string') {
                const stringValue = value.match(tagMatch)?.pop()?.toString().slice(1, -1);
                const tagValues = stringValue?.split(', ') ?? [];
                const tags = extractTags(tagValues);
                if (!_.isEqual(tags, values.tags)) setFieldValue('tags', tags);
              }
            }}
          />
          <textarea
            className="raw-editor"
            onChange={(e) => setState({ ...state, rawHtml: e.target.value })}
            value={state.rawHtml}
          />
        </>
      )}
      <Error error={touched[fieldName!] ? (errors[fieldName!] as string) : ''} />
    </styled.Wysiwyg>
  );
};
