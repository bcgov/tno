import styled from 'styled-components';

export const ContentList = styled.div<{ scrollWithin: boolean }>`
  .group-title {
    border-bottom: 1px solid ${(props) => props.theme.css.border};
    margin-bottom: 0;
  }
  ${(props) => (props.scrollWithin ? 'overflow-y: auto;' : '')}
`;
