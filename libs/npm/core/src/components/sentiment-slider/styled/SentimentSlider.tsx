import styled from 'styled-components';

export const SentimentSlider = styled.div`
  display: flex;
  flex-direction: column;
  margin-bottom: 0.5rem;

  > div {
    display: flex;
    flex-direction: row;
    gap: 0.5rem;
    width: 100%;
    overflow: hidden;
    padding: 0.5rem 0;

    > div {
      width: 100%;
      margin: 0.5rem 1rem;
    }

    > svg {
      padding: 0.5rem;

      &:hover {
        color: red;
        cursor: pointer;
      }
    }
  }
`;
