import React from 'react';

import * as styled from './FooterStyled';

interface IFooterProps extends React.HTMLAttributes<HTMLElement> {}

/**
 * Footer provides a footer element to place at the bottom of the page.
 * By default includes links to 'Disclaimer, Privacy, Assessibility, Copyright, and Contact Us'.
 * @param param0 Footer element attributes.
 * @returns Footer component.
 */
export const Footer: React.FC<IFooterProps> = ({ children, ...rest }) => {
  return (
    <styled.Footer {...rest}>
      {!React.isValidElement(children) ? (
        <div>
          <a
            href="https://www2.gov.bc.ca/gov/content?id=79F93E018712422FBC8E674A67A70535"
            target="_blank"
            rel="noreferrer"
          >
            Disclaimer
          </a>
          <a
            href="https://www2.gov.bc.ca/gov/content?id=9E890E16955E4FF4BF3B0E07B4722932"
            target="_blank"
            rel="noreferrer"
          >
            Privacy
          </a>
          <a
            href="https://www2.gov.bc.ca/gov/content?id=E08E79740F9C41B9B0C484685CC5E412"
            target="_blank"
            rel="noreferrer"
          >
            Accessibility
          </a>
          <a
            href="https://www2.gov.bc.ca/gov/content?id=1AAACC9C65754E4D89A118B875E0FBDA"
            target="_blank"
            rel="noreferrer"
          >
            Copyright
          </a>
          <a
            href="https://www2.gov.bc.ca/gov/content?id=6A77C17D0CCB48F897F8598CCC019111"
            target="_blank"
            rel="noreferrer"
          >
            Contact Us
          </a>
        </div>
      ) : (
        <>{children}</>
      )}
    </styled.Footer>
  );
};
