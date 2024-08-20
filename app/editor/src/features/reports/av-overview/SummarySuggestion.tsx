import { useFormikContext } from 'formik';
import { IAVOverviewInstanceModel } from 'tno-core';

import * as styled from './styled';
import { ISectionSummary } from './utils';

export interface ISummarySuggestionProps {
  show?: boolean;
  sectionIndex: number;
  itemIndex: number;
  suggestions?: ISectionSummary[];
  onClose: (itemIndex: number) => void;
}

/**
 * Provides a popup containing summaries to pick from.
 * @param param0 Component properties
 * @returns component
 */
export const SummarySuggestion: React.FC<ISummarySuggestionProps> = ({
  show,
  sectionIndex,
  itemIndex,
  suggestions,
  onClose,
}) => {
  const { values, setFieldValue } = useFormikContext<IAVOverviewInstanceModel>();

  const item = values.sections[sectionIndex].items[itemIndex];
  const items =
    suggestions?.filter(
      (s) => s.key !== `${sectionIndex}-${itemIndex}` && s.text.startsWith(item.summary),
    ) ?? [];

  return (
    <>
      {items.length > 0 && show && (
        <styled.AutoCompleteContainer>
          {items.map((suggestion: ISectionSummary) => {
            return (
              <styled.AutoCompleteItem key={suggestion.key}>
                <styled.AutoCompleteItemButton
                  onClick={() => {
                    setFieldValue(
                      `sections.${sectionIndex}.items.${itemIndex}.summary`,
                      suggestion.text,
                    );
                    onClose(itemIndex);
                  }}
                >
                  {suggestion.text}
                </styled.AutoCompleteItemButton>
              </styled.AutoCompleteItem>
            );
          })}
        </styled.AutoCompleteContainer>
      )}
    </>
  );
};
