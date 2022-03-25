import { FormikCheckbox } from 'components/formik';
import { useFormikContext } from 'formik';
import { IActionValueModel } from 'hooks';
import { useLookup } from 'store/hooks';

import { IContentForm } from './interfaces';

export interface IActionCheckbox extends React.HTMLAttributes<HTMLInputElement> {
  /** the action name to pass down to the checkbox */
  name: string;
}

/** This component handles the logic that checks the actions array to see whther the content has specific actions
 *  associated to it or not.
 */
export const ActionCheckbox: React.FC<IActionCheckbox> = ({ name, onClick }) => {
  const { setFieldValue, values } = useFormikContext<IContentForm>();
  const [{ actions: lActions }] = useLookup();
  let actions = values.actions;

  /** find the id corresponding to this action in order to send it to the API */
  const actionId = lActions.find((x) => x.name === name)?.id ?? -1;
  /** check the value attribute of the action to determine the value of checked */
  const checked = !!actions.find((x: IActionValueModel) => x.name === name && x.value === 'true');
  /** find the index of the action in order to alter the value, returns -1 if it does not exist */
  const index = actions.findIndex((x) => x.name === name);
  return (
    <FormikCheckbox
      className="chk"
      name={name}
      labelRight
      label={name}
      onClick={onClick}
      checked={checked}
      onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
        if (index !== -1) {
          actions[index].value = String(e.target.checked);
          setFieldValue('actions', actions);
        } else {
          actions.push({ id: actionId, value: String(e.target.checked), name: name });
          setFieldValue('actions', actions);
        }
      }}
    />
  );
};
