import styled from 'styled-components';

export const DroppableContentContainer = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.25rem;

  .drag-row {
    border: solid 1px ${(props) => props.theme.css.lightVariantColor};
    border-radius: 0.25rem;
    background-color: white;
  }
`;
