import styled from 'styled-components';

import { ITableBuilderProps } from '../TableBuilder';

export const TableBuilder = styled.table<ITableBuilderProps>`
  display: ${(props) => (props.isHidden ? 'none' : 'inherit')};

  > thead {
    display: ${(props) => (!props.showHeader ? 'none' : 'inherit')};
  }
`;
