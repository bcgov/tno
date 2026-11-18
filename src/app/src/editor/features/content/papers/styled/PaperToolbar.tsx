import styled from 'styled-components';

export const PaperToolbar = styled.div`
  .select {
    max-width: 32em;

    &#sel-mediaTypeIds {
      .rs__value-container {
        max-width: 220px;
      }
    }
  }

  svg.action-button.button-preview {
    width: 2rem;
    height: 2rem;
  }
`;
