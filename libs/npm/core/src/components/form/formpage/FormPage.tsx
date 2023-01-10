import { InputHTMLAttributes } from 'react';

import * as styled from './styled';

export interface IFormPageProps extends InputHTMLAttributes<HTMLDivElement> {
  /** bypass the default min width */
  minWidth?: string;
  /** bypass the default max width */
  maxWidth?: string;
  /** remove padding from the FormPage */
  noPadding?: boolean;
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
