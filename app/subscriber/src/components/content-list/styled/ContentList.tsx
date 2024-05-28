import styled from 'styled-components';

export const ContentList = styled.div<{ scrollWithin: boolean }>`
  .group-title {
    font-size: 1.15rem;
    border-bottom: 1px solid ${(props) => props.theme.css.border};
    margin-bottom: 0;
  }
  ${(props) => (props.scrollWithin ? 'overflow-y: auto;' : '')}
`;
