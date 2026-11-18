import styled from 'styled-components';

export const ChartTemplatePreview = styled.div`
  display: flex;
  flex-direction: column;

  padding: 0.5rem;
  background: ${(props) => props.theme.css.beigeBackgroundColor};
  border-radius: 0.5rem;

  .preview-image {
    background: white;
  }
`;
