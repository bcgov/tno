import styled from 'styled-components';

import { IRadioGroupProps } from '..';

export const RadioGroup = styled.div<IRadioGroupProps<any>>`
  display: flex;
  flex-direction: column;

  .required:after {
    content: ' *';
    color: ${(props) => props.theme.css.dangerColor};
  }

  & > div {
    display: flex;
    flex-direction: ${(props) => (props.direction === 'row' ? 'row' : 'column')};
  }
`;
