import { FormPage } from 'components/formpage';
import styled from 'styled-components';

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

  div.row {
    cursor: pointer;

    div.column {
      overflow: hidden;
    }
  }

  .table {
    max-height: calc(100% - 120px);
    min-height: 200px;
  }
`;
