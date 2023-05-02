import styled from 'styled-components';
import { FormPage } from 'tno-core';

export const WorkOrderList = styled(FormPage)`
  .h-createdOn {
    text-align: center;
    align-items: center;
    justify-content: center;
  }

  .h-updatedOn {
    text-align: center;
    align-items: center;
    justify-content: center;
  }

  .link {
    cursor: pointer;
    color: ${(props) => props.theme.css.primaryColor};

    &:hover {
      color: ${(props) => props.theme.css.primaryLightColor};
    }
  }

  div.column {
    overflow: hidden;
  }
`;
