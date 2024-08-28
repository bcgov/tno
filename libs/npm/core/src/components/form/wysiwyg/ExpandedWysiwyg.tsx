import React from 'react';
import { FaX } from 'react-icons/fa6';
import { IStateProps, IWysiwygProps, Wysiwyg } from './Wysiwyg';
import { useWysiwygContext } from './WysiwygContextProvider';

interface IExpandWysiwygProps extends IWysiwygProps {}

export const ExpandedWysiwyg: React.FC<IExpandWysiwygProps> = (props) => {
  const { expandedState, setExpandedState, setExpand, expand, normalState, setNormalState } =
    useWysiwygContext();

  const handleExit = () => {
    setExpand(false);
    if (expandedState.html !== normalState.html) {
      setExpandedState({ ...expandedState, html: props.value ?? '' });
    }
    if (normalState.html !== expandedState.html) {
      setNormalState(expandedState);
    }
  };

  return (
    <>
      <FaX className="exit" onClick={handleExit} />
      <Wysiwyg {...props} value={expandedState.html} />
    </>
  );
};
