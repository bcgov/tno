import React from 'react';
import { Text } from 'tno-core';

export interface IDraftTextProps {
  name: string;
  label?: string;
  placeholder?: string;
  width?: string;
  /** The canonical serialized form of the stored value. */
  canonical: string;
  /** Receives the raw text on every keystroke; the caller parses it. */
  onText: (text: string) => void;
}

/**
 * A text input for fields whose value is parsed (comma lists, field=value maps): while typing
 * it shows exactly what was typed - so separators like a trailing comma survive - and only
 * snaps back to the canonical serialized form on blur.
 */
export const DraftText: React.FC<IDraftTextProps> = ({
  name,
  label,
  placeholder,
  width,
  canonical,
  onText,
}) => {
  const [draft, setDraft] = React.useState<string | null>(null);
  return (
    <Text
      name={name}
      label={label}
      placeholder={placeholder}
      width={width}
      value={draft ?? canonical}
      onChange={(e) => {
        setDraft(e.target.value);
        onText(e.target.value);
      }}
      onBlur={() => setDraft(null)}
    />
  );
};
