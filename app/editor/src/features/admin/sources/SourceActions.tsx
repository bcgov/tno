import { FormikCheckbox } from 'components/formik';
import { getIn, useFormikContext } from 'formik';
import { useNamespace } from 'hooks';
import { ISourceActionModel, ISourceModel } from 'hooks/api-editor';
import React from 'react';
import { useLookup } from 'store/hooks';

export const SourceActions: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<ISourceModel>();
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

  const options = sourceActions.map((a) => {
    const index = actions.findIndex((da) => da.id === a.id);
    const found = actions[index];
    return (
      <FormikCheckbox
        key={a.id}
        label={a.name}
        name={field('value', index)}
        value="true"
        checked={found?.value === 'true'}
        onChange={(e) => {
          const checked = e.currentTarget.checked;
          setFieldValue(field('value', index), checked ? 'true' : 'false');
        }}
      />
    );
  });

  return <>{options}</>;
};
