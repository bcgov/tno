import React from 'react';
import { Checkbox, ITransferObject, Show, Text } from 'tno-core';

export interface ITransferObjectsProps {
  /** A name to prepend to each input control. */
  name: string;
  /** Label to display at top of section. */
  label?: string;
  /** An array of items that can be transferred. */
  items: ITransferObject[];
  /** An array of names that cannot be duplicated. */
  names: string[];
  /** Whether to transfer the ownership of the objects.  If false, it will copy. */
  transferOwnership: boolean;
  /** When there are no objects, this message is displayed. */
  emptyMessage: string;
  /** Event fires when a transfer object is selected. */
  onSelect: (id: number, checked: boolean) => void;
  /** Event fires when a transfer object name is modified. */
  onChangeName: (id: number, name: string) => void;
}

/**
 * Provides an array of rows to display transfer objects.
 * Allows user to select and edit each row.
 * @param param0 Component properties.
 * @returns Component.
 */
export const TransferObjects: React.FC<ITransferObjectsProps> = ({
  name,
  label,
  items,
  names,
  transferOwnership,
  emptyMessage,
  onSelect,
  onChangeName,
}) => {
  return (
    <div className="grid-section">
      {label && <label>{label}</label>}
      <div>
        {items.map((n) => {
          const showNewName = n.checked && names.some((name) => name === n.originalName);
          const mustChangeName =
            showNewName && n.newName && names.some((name) => name === n.newName);
          return (
            <React.Fragment key={n.originalId}>
              <div>
                <Checkbox
                  name={`${name}-include-${n.originalId}`}
                  checked={n.checked}
                  onChange={(e) => {
                    onSelect(n.originalId, e.target.checked);
                  }}
                />
              </div>
              <div>{n.originalName}</div>
              <div>{n.subscribeOnly ? 'subscribe' : transferOwnership ? 'transfer' : 'copy'}</div>
              <div>
                <Show visible={!n.subscribeOnly && showNewName}>
                  <Text
                    name={`${name}-name-${n.originalId}`}
                    placeholder="New name required"
                    required={showNewName}
                    className={mustChangeName ? 'error' : ''}
                    onChange={(e) => {
                      onChangeName(n.originalId, e.target.value);
                    }}
                  />
                </Show>
              </div>
            </React.Fragment>
          );
        })}
      </div>
      <Show visible={!items.length}>
        <p>{emptyMessage}</p>
      </Show>
    </div>
  );
};
