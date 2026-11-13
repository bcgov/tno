import styled from 'styled-components';

export const ChartTemplatePreviewOptions = styled.div`
  display: flex;
  flex-direction: column;

  padding: 0.5rem;
  background: ${(props) => props.theme.css.beigeBackgroundColor};
  border-radius: 0.5rem;

  > div:nth-child(2) {
    justify-content: space-between;
    justify-items: stretch;
    padding: 0 0.5rem;

    > div {
      border: solid 1px ${(props) => props.theme.css.lightVariantColor};
      border-radius: 0.25rem;
      padding: 0.5rem;
    }
  }
`;
