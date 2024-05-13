import React, { createContext, ReactNode, useContext, useState } from 'react';
import { IFolderModel } from 'tno-core';

interface IFolderContext {
  folder: IFolderModel | undefined;
  setFolder: (folder: IFolderModel) => void;
}

// Create the context with a default value
const FolderContext = createContext<IFolderContext | undefined>(undefined);

// Create a provider component
interface IFolderProviderProps {
  children: ReactNode;
}

const FolderProvider: React.FC<IFolderProviderProps> = ({ children }) => {
  const [folder, setFolder] = useState<IFolderModel>();

  return <FolderContext.Provider value={{ folder, setFolder }}>{children}</FolderContext.Provider>;
};

const useFolderContext = (): IFolderContext => {
  const context = useContext(FolderContext);
  if (context === undefined) {
    throw new Error('useFolder must be used within a FolderProvider');
  }
  return context;
};

export { FolderContext, FolderProvider, useFolderContext };
