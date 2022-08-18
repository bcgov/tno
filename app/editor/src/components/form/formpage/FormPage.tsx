import { InputHTMLAttributes } from 'react';

import * as styled from './styled';

export interface IFormPageProps extends InputHTMLAttributes<HTMLDivElement> {
  /** bypass the default min width */
  bypassMinWidth?: boolean;
  /** include contents of form page */
  children: React.ReactNode;
}
/** Provides a consistent white background "page" used throughout the application */
export const FormPage: React.FC<IFormPageProps> = ({ children, ...rest }) => {
  return <styled.FormPage {...rest}>{children}</styled.FormPage>;
};
