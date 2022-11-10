import { useFormikContext } from 'formik';
import { IContentModel } from 'hooks';
import _ from 'lodash';
import React from 'react';
import ReactQuill from 'react-quill';
import { useLookup } from 'store/hooks';
import { Show } from 'tno-core';

import { CustomToolbar } from './CustomToolbar';
import * as styled from './styled';

export interface IWysiwygProps {}
/**
 * A WYSIWYG editor for the content summary form
 * @returns A WYSIWYG editor for the content summary form
 */
export const Wysiwyg: React.FC<IWysiwygProps> = ({}) => {
  const { values, setFieldValue } = useFormikContext<IContentModel>();
  const [{ tags }] = useLookup();

  const [state, setState] = React.useState({
    html: values.summary ?? '',
    rawHtml: values.summary ?? '',
  });
  const [showRaw, setShowRaw] = React.useState(false);
  // pattern match content between but not including [ and ]</p>
  const tagMatch = /(?<=\[)(.*?)(?=\]<\/p>)/g;

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

  const extractTags = (values: string[]) => {
    return tags
      .filter((tag) => values.some((value: string) => value.toLowerCase() === tag.id.toLowerCase()))
      .map((tag) => tag);
  };

  const stripHtml = () => {
    // strip html from string
    let doc = new DOMParser().parseFromString(values.summary, 'text/html');
    setFieldValue('summary', doc.body.textContent || '');
    setState({ ...state, html: doc.body.textContent || '' });
  };

  const handleChange = (html: string) => {
    setState({ ...state, html: html });
    setFieldValue('summary', html);
  };

  const modules = {
    toolbar: {
      container: '#toolbar',
      handlers: {},
    },
  };

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
    <styled.Wysiwyg>
      <CustomToolbar onClickRaw={onClickRaw} onClickRemoveFormat={stripHtml} />
      <Show visible={!showRaw}>
        <ReactQuill
          className="editor"
          value={!!state.html ? state.html : values.summary}
          onChange={handleChange}
          theme="snow"
          modules={modules}
          formats={formats}
          onBlur={(e) => {
            const value = values.summary;
            if (!!value) {
              const stringValue = value.match(tagMatch)?.toString();
              const tagValues = stringValue?.split(', ') ?? [];
              const tags = extractTags(tagValues);
              if (!_.isEqual(tags, values.tags)) setFieldValue('tags', tags);
            }
          }}
        />
      </Show>
      <Show visible={showRaw}>
        <textarea
          className="raw-editor"
          onChange={(e) => setState({ ...state, rawHtml: e.target.value })}
          value={state.rawHtml}
        />
      </Show>
    </styled.Wysiwyg>
  );
};
