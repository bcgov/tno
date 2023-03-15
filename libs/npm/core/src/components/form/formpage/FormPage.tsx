import { InputHTMLAttributes } from 'react';

import * as styled from './styled';

export interface IFormPageProps extends InputHTMLAttributes<HTMLDivElement> {
  /** include contents of form page */
  children: React.ReactNode;
}

/** Provides a consistent white background "page" used throughout the application */
export const FormPage: React.FC<IFormPageProps> = ({ children, className, ...rest }) => {
  return (
    <styled.FormPage {...rest} className={`form-page${className ? ` ${className}` : ''}`}>
      {children}
    </styled.FormPage>
  );
};
