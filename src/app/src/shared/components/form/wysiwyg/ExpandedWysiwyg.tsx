import React from 'react';
import { FaX } from 'react-icons/fa6';

import { IStateProps, IWysiwygProps, Wysiwyg } from './Wysiwyg';

interface IExpandWysiwygProps extends IWysiwygProps {
  setExpand: React.Dispatch<React.SetStateAction<boolean>>;
  expand: boolean;
  normalState: IStateProps;
  setNormalState: React.Dispatch<React.SetStateAction<IStateProps>>;
}

/**
 * ExpandedWysiwyg component that appears in full screen.
 */
export const ExpandedWysiwyg: React.FC<IExpandWysiwygProps> = (props) => {
  const handleExit = () => {
    props.setExpand(false);
  };

  return (
    <>
      <FaX className="exit" onClick={handleExit} />
      <Wysiwyg
        {...props}
        value={props.normalState.html}
        onChange={(html, editor) => {
          props.setNormalState({ html, text: html });
          props.onChange?.(html, editor);
        }}
      />
    </>
  );
};
