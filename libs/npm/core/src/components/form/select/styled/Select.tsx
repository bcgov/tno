import styled from 'styled-components';

import { Col } from '../../../flex';

export const Select = styled(Col)`
  .required:after {
    content: ' *';
    color: ${(props) => props.theme.css.dangerColor};
  }

  .rs__clear-indicator {
    cursor: pointer;
  }

  .rs__clear-indicator:hover {
    color: ${(props) => props.theme.css.dangerColor};
  }

  & > input {
    width: 0px !important; // Required to stop the input from using 100% width and causing horizontal overflow issues.
  }
`;
