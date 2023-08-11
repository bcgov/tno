import styled from 'styled-components';

export const PaperFilter = styled.div`
  .select {
    max-width: 32em;

    &#sel-productIds {
      .rs__value-container {
        max-width: 220px;
      }
    }
  }

  svg.action-button.btn-preview {
    width: 3rem;
    height: 3rem;
  }
`;
