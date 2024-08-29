import React from 'react';
import { FaX } from 'react-icons/fa6';
import { IStateProps, IWysiwygProps, Wysiwyg } from './Wysiwyg';

interface IExpandWysiwygProps extends IWysiwygProps {
  expandedState: IStateProps;
  setExpandedState: React.Dispatch<React.SetStateAction<IStateProps>>;
  setExpand: React.Dispatch<React.SetStateAction<boolean>>;
  expand: boolean;
  normalState: IStateProps;
  setNormalState: React.Dispatch<React.SetStateAction<IStateProps>>;
}

/**
 * ExpandedWysiwyg component that appears in full screen.
 */
export const ExpandedWysiwyg: React.FC<IExpandWysiwygProps> = (props) => {
  // sync the expanded wysiwyg state with the normal wysiwyg state when opening the expanded wysiwyg
  React.useEffect(() => {
    if (
      (!props.expandedState.html && !!props.normalState.html) ||
      props.expandedState.html !== props.normalState.html
    ) {
      props.setExpandedState({ ...props.expandedState, html: props.normalState.html });
    }
    // only want to run this effect when the expand prop changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [props.expand]);

  const handleExit = () => {
    props.setExpand(false);
  };

  return (
    <>
      <FaX className="exit" onClick={handleExit} />
      <Wysiwyg {...props} value={props.expandedState.html} />
    </>
  );
};
