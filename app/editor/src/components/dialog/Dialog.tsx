import './dialog.scss';

import { Dialog as HDialog } from '@headlessui/react';
import { Button } from 'components';
import { IResponseErrorModel } from 'hooks';
import React from 'react';

export interface IDialogProps extends React.HTMLAttributes<HTMLElement> {
  open: boolean;
  onClose(value: boolean): void;
  title?: string;
  description?: string;
  message?: string;
  data?: IResponseErrorModel;
}

export const Dialog: React.FC<IDialogProps> = ({
  title = 'Information',
  description,
  message,
  data,
  open,
  onClose,
}) => {
  if (data) {
    description = data.title;
    message =
      'Errors with the following properties: ' +
      Object.keys(data.errors)
        .filter((key) => key.startsWith('$.'))
        .map((key) => `"${key.replace('$.', '')}"`)
        .join(', ');
  }

  return (
    <HDialog as="div" open={open} onClose={() => onClose(false)}>
      <div>
        <HDialog.Overlay className="dialog-overlay" />
        <HDialog.Title>{title}</HDialog.Title>
        {description && <HDialog.Description>{description}</HDialog.Description>}
        {message && <p>{message}</p>}

        <Button onClick={() => onClose(false)}>Close</Button>
      </div>
    </HDialog>
  );
};
