import * as styled from './AreaStyled';

export interface IAreaProps {
  children?: React.ReactNode;
}

/**
 * Custom div component used to contain the form for the Content View.
 * @returns Container area for the content form.
 */
export const Area: React.FC<any> = ({ children }) => {
  return <styled.Area>{children}</styled.Area>;
};
