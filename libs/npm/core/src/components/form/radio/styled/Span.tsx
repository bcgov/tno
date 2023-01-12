import styled from 'styled-components';

export const Span = styled.span<any>`
  display: flex;
  flex-direction: ${(props) => props.flexRow && 'row'};
  padding-bottom: ${(props) => props.spaceUnderRadio && '10%'};
`;
