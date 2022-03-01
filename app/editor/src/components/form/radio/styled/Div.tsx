import styled from 'styled-components';

export const Div = styled.div<any>`
  display: flex;
  flex-direction: ${(props) => (props.direction === 'row' ? 'row' : 'column')};
`;
