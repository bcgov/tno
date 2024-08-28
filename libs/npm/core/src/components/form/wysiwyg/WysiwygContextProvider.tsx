import React, { createContext, ReactNode, useContext, useState } from 'react';
import { IStateProps } from './Wysiwyg';

interface IWysiwygContext {
  setExpandedState: (state: IStateProps) => void;
  expandedState: IStateProps;
  setNormalState: (state: IStateProps) => void;
  normalState: IStateProps;
  expand: boolean;
  setExpand: (expand: boolean) => void;
}

const WysiwygContext = createContext<IWysiwygContext | undefined>(undefined);

export const useWysiwygContext = (): IWysiwygContext => {
  const context = useContext(WysiwygContext);
  if (!context) {
    throw new Error('Wysiwyg context must be used within a WyswigProvider');
  }
  return context;
};

interface IWysiwygProvider {
  children: ReactNode;
}

export const WysiwygProvider: React.FC<IWysiwygProvider> = ({ children }) => {
  const [expandedState, setExpandedState] = useState<IStateProps>({ html: '', text: '' });
  const [normalState, setNormalState] = useState<IStateProps>({ html: '', text: '' });
  const [expand, setExpand] = useState<boolean>(false);

  const value = { setExpandedState, expandedState, setNormalState, normalState, expand, setExpand };

  return <WysiwygContext.Provider value={value}>{children}</WysiwygContext.Provider>;
};
