import styled from 'styled-components';
import { Col } from 'tno-core/dist/components/flex';

export const Select = styled(Col)`
  .required:after {
    content: ' *';
    color: ${(props) => props.theme.css.dangerColor};
  }
  margin-right: 0.5em;
  .select-container {
    width: 100%;
  }
  .input-container {
    position: relative;
    .clear {
      :hover {
        color: ${(props) => props.theme.css.dangerColor};
        cursor: pointer;
      }
      position: absolute;
      right: 55px;
      align-self: center;
      font-weight: 500;
      color: #cccccc;
    }
  }

  & > input {
    width: 0px !important; // Required to stop the input from using 100% width and causing horizontal overflow issues.
  }
`;
