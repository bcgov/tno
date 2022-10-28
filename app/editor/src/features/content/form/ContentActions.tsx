import { Checkbox } from 'components/form';
import { FormikCheckbox, FormikText, FormikTextArea } from 'components/formik';
import { getIn, useFormikContext } from 'formik';
import { ActionName, ContentTypeName, useNamespace } from 'hooks';
import { IActionModel, IContentActionModel, ValueType } from 'hooks/api-editor';
import React from 'react';
import { useLookup } from 'store/hooks';

import { IContentForm } from './interfaces';

export interface IContentActionsProps {
  /** The name of the input and field assessor */
  name?: string;
  /**
   * Initialize the action array to ensure all values available are included in the form.
   * If you have more than a single ContentActions component, you should only initialize once.
   */
  init?: boolean;
  /**
   * Filter which actions you want included.
   */
  filter?: (action: IActionModel) => boolean;
  /** The type of content that is being displayed within the Content Form */
  contentType?: ContentTypeName;
}

export interface IContentActionCheckbox {
  id: number;
  value: boolean;
}

export const ContentActions: React.FC<IContentActionsProps> = ({
  name = 'actions',
  init,
  filter = () => true,
  contentType,
}) => {
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const [{ actions }] = useLookup();
  const { field } = useNamespace(name);

  const formActions: IContentActionModel[] = getIn(values, name, []);
  const [hidden, setHidden] = React.useState<IContentActionCheckbox[]>([]);

  React.useEffect(() => {
    // Needed this to reset the hidden values when new content is loaded.
    // Otherwise the checkboxes that have the extra input results in an invalid checked.
    setHidden((state) =>
      state.map((h) => {
        const found = formActions.find((a) => a.id === h.id);
        return { ...h, value: !!found?.value };
      }),
    );
  }, [values.id, formActions]);

  React.useEffect(() => {
    // Make sure the available actions are all included in the content.
    if (!!init && !!actions.length && values.actions.length !== actions.length) {
      const defaultActions = [...values.actions];
      actions.forEach((action) => {
        let found = values.actions.find((va) => va.id === action.id);
        if (found === undefined) {
          found = { ...action, value: action.defaultValue };
          defaultActions.push(found);
        }
        if (found.name === ActionName.Alert && contentType === ContentTypeName.PrintContent) {
          // Default PrintContent to not alert.
          found.value = 'false';
        }
        if (found.name === ActionName.FrontPage && contentType === ContentTypeName.Image) {
          found.value = 'true';
        }
      });
      setFieldValue(name, defaultActions);
    }
  }, [setFieldValue, actions, values.actions, init, name, contentType]);

  React.useEffect(() => {
    // When form action values are changed the hidden checkbox values must update so that the checkbox works correctly.
    setHidden((state) => {
      return formActions.map((a) => {
        const found = state.find((i) => i.id === a.id);
        return { id: a.id, value: !!a.value ? true : found?.value ?? false };
      });
    });
  }, [formActions]);

  const options = actions.filter(filter).map((a) => {
    const index = formActions.findIndex((ca) => ca.id === a.id);
    const found = formActions[index];
    return (
      <React.Fragment key={a.id}>
        {a.valueType === ValueType.Boolean && (
          <FormikCheckbox
            label={a.name}
            name={field('value', index)}
            value="true"
            checked={found?.value === 'true'}
            onChange={(e) => {
              const checked = e.currentTarget.checked;
              setFieldValue(field('value', index), checked ? 'true' : 'false');
            }}
          />
        )}
        {a.valueType !== ValueType.Boolean && (
          <Checkbox
            label={a.name}
            name={field('placeholder', index)}
            value={true}
            checked={!!hidden.find((h) => h.id === a.id)?.value}
            onChange={(e) => {
              const checked = e.currentTarget.checked;
              if (!checked) setFieldValue(field('value', index), '');
              setHidden(
                hidden.map((h) => {
                  if (h.id === a.id) return { ...h, value: checked };
                  return h;
                }),
              );
            }}
          />
        )}
        {a.valueType === ValueType.String && (
          <FormikText
            name={field('value', index)}
            label={a.valueLabel}
            disabled={!hidden.find((h) => h.id === a.id)?.value ?? true}
            required={!!hidden.find((h) => h.id === a.id)?.value}
          />
        )}
        {a.valueType === ValueType.Text && (
          <FormikTextArea
            name={field('value', index)}
            label={a.valueLabel}
            disabled={!hidden.find((h) => h.id === a.id)?.value ?? true}
            required={!!hidden.find((h) => h.id === a.id)?.value}
          />
        )}
        {a.valueType === ValueType.Numeric && (
          <FormikText
            name={field('value', index)}
            label={a.valueLabel}
            type="number"
            disabled={!hidden.find((h) => h.id === a.id)?.value ?? true}
            required={!!hidden.find((h) => h.id === a.id)?.value}
          />
        )}
      </React.Fragment>
    );
  });

  return <>{options}</>;
};
