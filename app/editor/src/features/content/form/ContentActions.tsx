import { getIn, useFormikContext } from 'formik';
import React from 'react';
import { FaHourglassHalf } from 'react-icons/fa';
import { useLookup, useSettings } from 'store/hooks';
import {
  Checkbox,
  ContentTypeName,
  FieldSize,
  FormikCheckbox,
  FormikText,
  FormikTextArea,
  IActionModel,
  IContentActionModel,
  Row,
  useNamespace,
  ValueType,
} from 'tno-core';

import { ActionNames } from './constants/actionsEnum';
import { IContentForm } from './interfaces';
import { StyledShortcutKey } from './styled';
import { getDefaultCommentaryExpiryValue } from './utils';

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
  /** A way to force actions to go to a new row. */
  addRowOn?: (action: IActionModel, index: number) => boolean;
}

export interface IContentActionCheckbox {
  id: number;
  value: boolean;
}

export const ContentActions = React.forwardRef<HTMLDivElement, IContentActionsProps>(
  ({ name = 'actions', init, filter, addRowOn }, ref) => {
    const { values, setFieldValue } = useFormikContext<IContentForm>();
    const [{ actions, holidays }] = useLookup();
    const { featuredStoryActionId, commentaryActionId, alertActionId } = useSettings();
    const { field } = useNamespace(name);

    const formActions: IContentActionModel[] = getIn(values, name, []);
    const [hidden, setHidden] = React.useState<IContentActionCheckbox[]>([]);

    // Default to only showing actions assigned to the content type.
    filter ??= (action: IActionModel) => action.contentTypes.includes(values.contentType);
    const options = actions.filter((a) => a.isEnabled && filter?.(a));
    addRowOn ??= (action: IActionModel, index: number) =>
      options.length > 4 && action.id === commentaryActionId;

    const toggle = (checked: boolean | string) => {
      // Check if checked is a boolean
      if (typeof checked === 'boolean') {
        return !checked ? '' : 'true';
      }

      // Handle the case where checked is a string
      const booleanValue = !JSON.parse(checked || 'false');
      return booleanValue ? 'true' : '';
    };

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
      if (!!init && alertActionId && !!actions.length && values.actions.length !== actions.length) {
        const defaultActions = [...values.actions];
        actions.forEach((action) => {
          let found = values.actions.find((va) => va.id === action.id);
          if (found === undefined) {
            found = { ...action, value: action.defaultValue };
            defaultActions.push(found);
          }
          if (found.id === alertActionId && values.contentType === ContentTypeName.PrintContent) {
            // Default PrintContent to not alert.
            found.value = 'false';
          } else if (
            found.id === featuredStoryActionId &&
            values.contentType === ContentTypeName.Image
          ) {
            found.value = 'true';
          }
        });
        setFieldValue(name, defaultActions);
      }
    }, [
      setFieldValue,
      actions,
      values.actions,
      init,
      name,
      values.contentType,
      featuredStoryActionId,
      alertActionId,
    ]);

    React.useEffect(() => {
      // When form action values are changed the hidden checkbox values must update so that the checkbox works correctly.
      setHidden((state) => {
        return formActions.map((a) => {
          const found = state.find((i) => i.id === a.id);
          return { id: a.id, value: !!a.value ? true : found?.value ?? false };
        });
      });
    }, [formActions]);

    const handleKeyDown = React.useCallback(
      (e: KeyboardEvent) => {
        const handleToggleAction = (actionName: string) => {
          const actionIndex = formActions.findIndex((action) => action.name === actionName);
          if (actionIndex !== -1) {
            const checked = formActions[actionIndex].value;
            const checkToggled = toggle(checked);
            const newValue =
              checkToggled === 'true' ? (actionName === 'Commentary' ? '3' : 'true') : '';
            setFieldValue(field('value', actionIndex), newValue ? newValue : checkToggled);
          }
        };

        if (e.ctrlKey) {
          switch (e.code) {
            case 'KeyM':
              handleToggleAction(ActionNames.Commentary);
              break;
            case 'KeyI':
              handleToggleAction(ActionNames.TopStory);
              break;
            case 'KeyU':
              e.preventDefault();
              handleToggleAction(ActionNames.FeaturedStory);
              break;
            case 'KeyL':
              e.preventDefault();
              handleToggleAction(ActionNames.NonQualifiedSubject);
              break;
            default:
              break;
          }
        }
      },
      [formActions, setFieldValue, field],
    );

    React.useEffect(() => {
      window.addEventListener('keydown', handleKeyDown);
      return () => {
        window.removeEventListener('keydown', handleKeyDown);
      };
    }, [handleKeyDown]);

    const inputs = options.map((a, rowIndex) => {
      const actionIndex = formActions.findIndex((ca) => ca.id === a.id);
      const found = formActions[actionIndex];
      const labelContent = (() => {
        if (a.name === 'Top Story') {
          return (
            <div>
              Top Stor<StyledShortcutKey>i</StyledShortcutKey>es
            </div>
          );
        } else if (a.name === 'Commentary') {
          return (
            <div>
              Co<StyledShortcutKey>m</StyledShortcutKey>mentary
            </div>
          );
        } else if (a.name === 'Featured Story') {
          return (
            <div>
              Feat<StyledShortcutKey>u</StyledShortcutKey>red Stories
            </div>
          );
        } else if (a.name === 'Non Qualified Subject') {
          return (
            <div>
              Non Qua<StyledShortcutKey>l</StyledShortcutKey>ified Subject
            </div>
          );
        } else {
          return a.name;
        }
      })();
      return (
        <React.Fragment key={a.id}>
          {addRowOn?.(a, rowIndex) && <div className="forceFlexRow"></div>}
          {a.valueType === ValueType.Boolean && (
            <FormikCheckbox
              label={labelContent}
              name={field('value', actionIndex)}
              value="true"
              checked={found?.value === 'true'}
              onChange={(e) => {
                const checked = e.currentTarget.checked;
                setFieldValue(field('value', actionIndex), checked ? 'true' : 'false');
              }}
            />
          )}
          {a.valueType !== ValueType.Boolean && (
            <Checkbox
              label={labelContent}
              name={field('placeholder', actionIndex)}
              value={true}
              checked={!!hidden.find((h) => h.id === a.id)?.value}
              onChange={(e) => {
                const checked = e.currentTarget.checked;
                if (!checked) setFieldValue(field('value', actionIndex), '');
                else {
                  setFieldValue(
                    field('value', actionIndex),
                    `${getDefaultCommentaryExpiryValue(values.publishedOn, holidays)}`,
                  );
                }
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
            <Row>
              {a.id === commentaryActionId && <FaHourglassHalf className="icon-indicator" />}
              <FormikText
                name={field('value', actionIndex)}
                disabled={!hidden.find((h) => h.id === a.id)?.value ?? true}
                required={!!hidden.find((h) => h.id === a.id)?.value}
                width={FieldSize.Tiny}
                className="small-txt"
              />
            </Row>
          )}
          {a.valueType === ValueType.Text && (
            <FormikTextArea
              name={field('value', actionIndex)}
              label={a.valueLabel}
              disabled={!hidden.find((h) => h.id === a.id)?.value ?? true}
              required={!!hidden.find((h) => h.id === a.id)?.value}
            />
          )}
          {a.valueType === ValueType.Numeric && (
            <FormikText
              name={field('value', actionIndex)}
              label={a.valueLabel}
              type="number"
              disabled={!hidden.find((h) => h.id === a.id)?.value ?? true}
              required={!!hidden.find((h) => h.id === a.id)?.value}
            />
          )}
        </React.Fragment>
      );
    });

    return <>{inputs}</>;
  },
);
