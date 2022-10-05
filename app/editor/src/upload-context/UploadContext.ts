import React from 'react';

import { IUploadFile, IUploadManager } from './interfaces';

export const UploadContext = React.createContext<IUploadManager>({
  upload: () => {
    throw Error('UploadContext has no Provider');
  },
});

export const UploadFilesContext = React.createContext<IUploadFile>({
  name: '',
  status: 'pending',
  loaded: 0,
  total: 0,
});

export const useUpload = (): IUploadManager => React.useContext(UploadContext);
