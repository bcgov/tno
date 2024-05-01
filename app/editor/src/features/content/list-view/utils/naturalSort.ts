import { IContentSearchResult } from 'store/slices';

function getPageSectionValue(row: IContentSearchResult) {
  // gnerate page:section string, keep it in the lowercase
  const value = `${row.original.page ? row.original.page : ''}:${
    row.original.section ? row.original.section : ''
  }`.toLowerCase();
  return value;
}

export function naturalSortValue(row: IContentSearchResult) {
  const pageSectionValue = getPageSectionValue(row);
  // Replace each segment of digits and non-digits with formatted strings
  // Digits are padded with zeros to the left to ensure correct natural sorting
  // Non-digits are left as is
  // eq. 'A2:sport' ->  'A0000000002:sport'
  // eq. 'A02:sport' -> 'A0000000002:sport'
  const formattedPageSectionValue = pageSectionValue.replace(/(\d+)|(\D+)/g, (_, $1, $2) =>
    $1 ? Number($1).toString().padStart(10, '0') : $2,
  );
  // we do consider source as the primary key for sorting
  return formattedPageSectionValue;
}
