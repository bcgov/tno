import styled from 'styled-components';

import { IRadioGroupProps } from '..';

export const RadioGroup = styled.div<IRadioGroupProps<any>>`
  display: flex;
  flex-direction: column;

  label {
    cursor: pointer;
  }

  .required:after {
    content: ' *';
    color: ${(props) => props.theme.css.dangerColor};
  }

  & > div {
    display: flex;
    flex-direction: ${(props) => (props.direction === 'col-row' ? 'row' : props.direction)};
    flex-wrap: wrap;

    & > :first-child {
      width: ${(props) => (props.direction === 'col-row' ? '100%' : '')};
    }

    & > :not(:first-child) {
      flex: 1;
    }
  }
`;
