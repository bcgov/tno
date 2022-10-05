import React from 'react';
import { toast } from 'react-toastify';
import { useStorage } from 'store/hooks';

import { IUploadFile, IUploadManager } from './interfaces';
import { UploadContext, UploadFilesContext } from './UploadContext';

export interface IUploadContextWrapperProps {
  children: React.ReactElement;
}

export const UploadContextWrapper: React.FC<IUploadContextWrapperProps> = ({ children }) => {
  const [file, setFile] = React.useState<IUploadFile>();
  const storage = useStorage();

  const toastId = React.useRef<any>(null);

  const upload = React.useCallback(
    (file: File) => {
      const data = new FormData();
      data.append('file', file);

      setFile({
        name: file.name,
        status: 'pending',
        loaded: 0,
        total: file.size,
      });
      storage
        .upload('/', file, false, undefined, (e: ProgressEvent) => {
          setFile((prev) => ({ ...prev!, loaded: e.loaded, total: e.total }));
          const progress = e.loaded / e.total;
          if (toastId.current === null) {
            toastId.current = toast.info('File Upload in Progress', {
              progress,
            });
          } else {
            toast.update(toastId.current, { progress });
          }
        })
        .then(() => {
          setFile((prev) => ({ ...prev!, status: 'success' }));
          toast.done(toastId.current);
        })
        .catch(() => {
          setFile((prev) => ({ ...prev!, status: 'failure' }));
        });
    },
    [storage],
  );

  const uploadManager: IUploadManager = React.useMemo(
    () => ({
      upload,
    }),
    [upload],
  );

  return (
    <UploadContext.Provider value={uploadManager}>
      <UploadFilesContext.Provider value={file!}>{children}</UploadFilesContext.Provider>
    </UploadContext.Provider>
  );
};
