import styled from 'styled-components';

export const Topic = styled.div`
  .type-not-applicable {
    font-style: italic;
  }
  .type-Proactive {
    color: #006600;
  }
  .type-Issues {
    color: #bb1111;
  }
  .type-disabled {
    font-style: italic;
  }
  .option-hint {
    display: inline-block;
    color: rgb(255, 255, 255);
    border-radius: 50%;
    font-size: 0.75rem;
    width: 1rem;
    height: 1rem;
    text-align: center;
    font-weight: bold;
    margin-right: 0.25rem;
    text-transform: uppercase;
  }
  .option-hint.type-Proactive {
    background-color: #006600;
  }
  .option-hint.type-Issues {
    background-color: #bb1111;
  }
  .option-hint.type-not-applicable {
    display: none;
  }
`;
