import { useFormikContext } from 'formik';
import { useLookup } from 'store/hooks';

import { IContentForm } from '../interfaces';

export interface IExtractTagsProps {
  setParsedTags?: React.Dispatch<React.SetStateAction<string[]>>;
}

export const useExtractTags = ({ setParsedTags }: IExtractTagsProps) => {
  const { setFieldValue } = useFormikContext<IContentForm>();
  const [{ tags }] = useLookup();

  return (name: string, text: string) => {
    setFieldValue(name, text);
    if (!setParsedTags) {
      return;
    }
    const noTagString = text.replace(/<\/?p>/g, '');
    const regex = new RegExp(/\[([^\]]*)\]$/);
    const t = noTagString.match(regex);
    const availableTags = tags.filter((t) => t.isEnabled).map((t) => t.code.toUpperCase());
    const parsedTags: string[] = [];
    t?.forEach((i: string) => {
      const s = i.replace(/[\][]/g, '').split(/[\s,]+/);
      s.forEach((j: string) => {
        if (availableTags.includes(j.toUpperCase())) {
          parsedTags.push(j.toUpperCase());
        }
      });
    });
    setParsedTags((tags) => Array.from(new Set(parsedTags)));
  };
};
