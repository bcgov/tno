import styled from 'styled-components';

import { Row } from '../../flex';

export const Breadcrumb = styled(Row)`
  padding: 0.25em;

  & p:first-child {
    margin-right: 0.25em;
  }

  .clickable {
    color: ${(props) => props.theme.css.primaryLightColor};
    cursor: pointer;
    :after {
      content: '\u00A0>\u00A0';
      color: ${(props) => props.theme.css.textColor};
      font-weight: bold;
    }
  }
  .current {
    cursor: default;
  }

  svg {
    margin-top: 1%;
  }

  label {
    margin-right: 1em;
  }
`;
