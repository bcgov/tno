import styled from 'styled-components';

export const Topic = styled.div`
  .type-not-applicable {
    font-style: italic;
  }
  .type-Proactive {
    color: ${(props) => props.theme.css.eotdEventTypeProactive};
  }
  .type-Issues {
    color: ${(props) => props.theme.css.eotdEventTypeIssues};
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
    background-color: ${(props) => props.theme.css.eotdEventTypeProactive};
  }
  .option-hint.type-Issues {
    background-color: ${(props) => props.theme.css.eotdEventTypeIssues};
  }
  .option-hint.type-not-applicable {
    display: none;
  }
`;
