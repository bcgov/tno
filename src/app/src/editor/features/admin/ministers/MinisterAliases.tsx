import { useFormikContext } from 'formik';
import React from 'react';
import { FaPlus, FaTrash } from 'react-icons/fa';

import {
  Button,
  ButtonVariant,
  Col,
  Error,
  FieldSize,
  Row,
  Text,
  type IMinisterModel,
} from '@/shared';

/** Maximum length of the stored aliases column (minister.aliases varchar(250)). */
export const MaxAliasesLength = 250;

/** Separator used to store the aliases in a single column. */
const AliasSeparator = ', ';

/**
 * Split the stored comma-separated aliases into individual values.
 * @param aliases The stored aliases.
 * @returns An array of aliases.
 */
export const parseAliases = (aliases?: string) =>
  (aliases ?? '')
    .split(',')
    .map((a) => a.trim())
    .filter((a) => !!a);

/**
 * Join the aliases into the stored comma-separated format, ignoring empty rows.
 * @param aliases The aliases.
 * @returns The stored aliases.
 */
export const serializeAliases = (aliases: string[]) =>
  aliases
    .map((a) => a.trim())
    .filter((a) => !!a)
    .join(AliasSeparator);

/**
 * Editable list of minister aliases, one text box per alias.
 * The aliases are stored as a single comma-separated value in the 'aliases' field.
 * @returns Component.
 */
export const MinisterAliases: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IMinisterModel>();
  const [rows, setRows] = React.useState<string[]>(() => parseAliases(values.aliases));

  // Reload the rows when the form value changes outside of this component (i.e. minister loaded).
  React.useEffect(() => {
    setRows((current) =>
      serializeAliases(current) === (values.aliases ?? '') ? current : parseAliases(values.aliases),
    );
  }, [values.aliases]);

  const update = (next: string[]) => {
    setRows(next);
    setFieldValue('aliases', serializeAliases(next));
  };

  const length = serializeAliases(rows).length;

  return (
    <Col className="frm-in minister-aliases" gap="0.5rem">
      <label
        data-tooltip-id="main-tooltip"
        data-tooltip-content="Names this minister is referred to by in content. The first alias is the label shown on My Minister."
      >
        Aliases
      </label>
      {rows.map((alias, index) => (
        <Row key={index} gap="0.5rem" nowrap className="field-row">
          <Text
            name={`alias-${index}`}
            aria-label={`Alias ${index + 1}`}
            width={FieldSize.Large}
            value={alias}
            // Commas separate the stored aliases, so they cannot be part of an alias.
            onChange={(e) =>
              update(rows.map((a, i) => (i === index ? e.target.value.replace(/,/g, '') : a)))
            }
          />
          <Button
            variant={ButtonVariant.danger}
            tooltip="Remove alias"
            aria-label={`Remove alias ${index + 1}`}
            onClick={() => update(rows.filter((_, i) => i !== index))}
          >
            <FaTrash />
          </Button>
        </Row>
      ))}
      <Row>
        <Button variant={ButtonVariant.secondary} onClick={() => setRows([...rows, ''])}>
          <FaPlus /> Add alias
        </Button>
      </Row>
      <Error
        error={
          length > MaxAliasesLength
            ? `Aliases cannot exceed ${MaxAliasesLength} characters in total (currently ${length}).`
            : undefined
        }
      />
    </Col>
  );
};
