import { FormikCheckbox } from 'components/formik';
import { getIn, useFormikContext } from 'formik';
import { useNamespace } from 'hooks';
import { IDataSourceModel, ISourceActionModel } from 'hooks/api-editor';
import React from 'react';
import { useLookup } from 'store/hooks';

import * as styled from './styled';

export const DataSourceActions: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();
  const [{ sourceActions }] = useLookup();
  const { field } = useNamespace('actions');

  const actions: ISourceActionModel[] = getIn(values, 'actions', []);

  React.useEffect(() => {
    // Make sure the available actions are all included in the data source.
    if (values.actions.length !== sourceActions.length) {
      const existingActions = [...values.actions];
      sourceActions.forEach((sa) => {
        const action: ISourceActionModel | undefined = values.actions.find((va) => va.id === sa.id);
        if (action === undefined) {
          existingActions.push(sa);
        }
      });
      setFieldValue('actions', existingActions);
    }
  }, [setFieldValue, sourceActions, values.actions]);

  const options = actions.map((a, i) => {
    return (
      <FormikCheckbox
        key={a.id}
        label={a.name}
        name={field('value', i)}
        value={true}
        checked={!!a?.value}
      />
    );
  });

  return <styled.DataSourceActions>{options}</styled.DataSourceActions>;
};
