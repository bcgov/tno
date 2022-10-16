import React from 'react';

import { FetchState, IUploadManager, IUploadState } from './interfaces';

export const UploadContext = React.createContext<IUploadManager>({
  upload: () => {
    throw Error('UploadContext has no Provider');
  },
});

export const UploadFilesContext = React.createContext<IUploadState>({
  name: '',
  status: FetchState.pending,
  loaded: 0,
  total: 0,
});

/** Hook used to integrate site-wide uploads on a given component */
export const useUpload = (): IUploadManager => React.useContext(UploadContext);
