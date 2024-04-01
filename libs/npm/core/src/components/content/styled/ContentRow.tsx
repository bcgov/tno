import styled from 'styled-components';

export const ContentRow = styled.div`
  display: flex;
  flex-direction: row;
  flex-wrap: nowrap;
  align-items: center;
  gap: 0.5rem;
  padding: 0.15rem;

  & div.headline {
    flex: 2 1 100%;
  }

  & div.byline,
  & div.section-page {
    flex: 1 1 15%;
  }

  & div.other-source {
    flex-basis: 5rem;
  }

  & div.published-on {
    flex-basis: 10rem;
  }

  & div.sentiment svg.tone-icon {
    margin-left: unset;
  }

  .frm-in {
    padding: 0;

    .number {
      text-align: right;
    }

    input {
      padding: 0.15rem 0.25rem;
    }
  }
`;
