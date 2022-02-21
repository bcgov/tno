import styled from 'styled-components';

export const StyledDiv = styled.div<any>`
  display: flex;
  flex-direction: ${(props) => (props.direction === 'row' ? 'row' : 'column')};
`;
