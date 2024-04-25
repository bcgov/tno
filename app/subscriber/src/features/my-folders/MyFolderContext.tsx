import React from 'react';
import { IFolderModel } from 'tno-core';

export interface IMyFolderContext {
  activeFolder: IFolderModel;
  setActiveFolder: (folder: IFolderModel) => void;
}

export interface IMyFolderContextProviderProps {
  children: React.ReactNode;
}

const MyFolderContext = React.createContext<IMyFolderContext | undefined>(undefined);

export const useMyFolderContext = () => {
  const context = React.useContext(MyFolderContext);
  if (!context) {
    throw new Error('useMyFolderContext must be used within a MyFolderContextProvider');
  }
  return context;
};

export const MyFolderContextProvider: React.FC<IMyFolderContextProviderProps> = ({ children }) => {
  const [activeFolder, setActiveFolder] = React.useState<IFolderModel | undefined>(undefined);

  return (
    <MyFolderContext.Provider value={{ activeFolder: activeFolder!, setActiveFolder }}>
      {children}
    </MyFolderContext.Provider>
  );
};
