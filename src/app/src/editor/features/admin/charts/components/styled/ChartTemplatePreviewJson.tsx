import styled from 'styled-components';

export const ChartTemplatePreviewJson = styled.div`
  display: flex;
  flex-direction: column;

  padding: 0.5rem;
  background: ${(props) => props.theme.css.beigeBackgroundColor};
  border-radius: 0.5rem;

  .code {
    .editor {
      background: white;

      max-width: unset;
    }
  }
`;
