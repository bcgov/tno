import { FormikCheckbox } from 'components/formik';
import { useFormikContext } from 'formik';
import { IActionValueModel } from 'hooks';

import { IContentForm } from './interfaces';

export interface IActionCheckbox extends React.HTMLAttributes<HTMLInputElement> {
  /** the action name to pass down to the checkbox */
  name: string;
  /** the id of the corresponding action as it is in the database */
  actionId: number;
  /** the label for the checkbox field */
  label?: string;
}

/** This component handles the logic that checks the actions array to see whther the content has specific actions
 *  associated to it or not.
 */
export const ActionCheckbox: React.FC<IActionCheckbox> = ({ name, label, actionId, onClick }) => {
  const { setFieldValue, values } = useFormikContext<IContentForm>();
  let actions = values.actions;
  return (
    <FormikCheckbox
      className="chk"
      name={name}
      labelRight
      label={label}
      onClick={onClick}
      checked={!!actions.find((x: IActionValueModel) => x.id === actionId && x.value === 'true')}
      onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
        if (actions.find((x: IActionValueModel) => x.id === actionId)) {
          const index = actions.findIndex((x) => x.id === actionId);
          actions[index].value = String(e.target.checked);
          setFieldValue('actions', actions);
        } else {
          actions.push({ id: actionId, value: String(e.target.checked) });
          setFieldValue('actions', actions);
        }
      }}
    />
  );
};
