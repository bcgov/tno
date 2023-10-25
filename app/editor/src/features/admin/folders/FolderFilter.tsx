import React from 'react';
import { useAdminStore } from 'store/slices';
import { IconButton, Row, Text } from 'tno-core';

interface IAdminFilterProps {
  onFilterChange?: (value: string) => void;
  onSearch?: (value: string) => void;
}

export const FolderFilter: React.FC<IAdminFilterProps> = ({ onFilterChange, onSearch }) => {
  const [{ folderFilter }, { storeFolderFilter }] = useAdminStore();

  return (
    <Row className="filter-bar" justifyContent="center">
      <Text
        onChange={(e) => {
          storeFolderFilter(e.target.value);
          onFilterChange?.(e.target.value);
        }}
        onKeyUp={(e) => {
          if (e.code === 'Enter') onSearch?.(folderFilter);
        }}
        placeholder="Search by keyword"
        name="search"
        value={folderFilter}
      >
        {!!onSearch && (
          <IconButton
            iconType="search"
            onClick={() => {
              onSearch?.(folderFilter);
            }}
          />
        )}
      </Text>
      <IconButton
        iconType="reset"
        onClick={() => {
          storeFolderFilter('');
          onFilterChange?.('');
          onSearch?.('');
        }}
      />
    </Row>
  );
};
