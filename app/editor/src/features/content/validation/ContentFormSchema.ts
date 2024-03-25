import { ContentStatusName, ContentTypeName } from 'tno-core';
import { array, date, number, object, string } from 'yup';

import { IContentForm } from '../form/interfaces';

export const ContentFormSchema = object<IContentForm>().shape(
  {
    mediaTypeId: number()
      .required('Media Type is required.')
      .notOneOf([0], 'Media Type is required.'),
    sourceId: number().when('tempSource', (value: number[]) => {
      if (value[0] === undefined)
        return number().required('Either source or other source is required.');
      return number();
    }),
    prep: string().test('timeTrackings', 'Prep time is required', (value, context) => {
      const parent: IContentForm = context.parent;
      if (
        parent.contentType === ContentTypeName.AudioVideo &&
        parent.status !== ContentStatusName.Draft
      ) {
        const totalEffort = parent.timeTrackings.reduce(
          (result, entry) => result + entry.effort,
          0,
        );
        if (!totalEffort || !parent.timeTrackings.some((entry) => entry.id === 0))
          return context.createError({ message: 'Prep time is required' });
      }
      return context.resolve(true);
    }),
    tempSource: string().when('sourceId', (value: string[]) => {
      if (value[0] === undefined)
        return string().required('Either source or other source is required.');
      return string();
    }),
    publishedOn: date().required('Published On is a required field.'),
    // TODO: Summary should not be empty.
    summary: string().when('contentType', (value: string[]) => {
      if (value[0] === ContentTypeName.AudioVideo || value[0] === ContentTypeName.Image) {
        return number().when('mediaTypeId', (value: number[]) => {
          // Summary is not a required field when content is tagged as News Radio or Events media type
          // TODO: This will break eventually because of hardcoded values.
          if (value[0] !== 4 && value[0] !== 9)
            return string().trim().required('Summary is a required field.');
          return string();
        });
      }
      return string();
    }),
    body: string().when('contentType', (value: string[]) => {
      if (value[0] !== ContentTypeName.AudioVideo && value[0] !== ContentTypeName.Image)
        return string().trim().required('Body is a required field.');
      return string();
    }),
    // TODO: Headline should not be empty.
    headline: string().required('Headline is a required field.'),
    tonePools: string().when('contentType', (values: string[]) => {
      if (values[0] !== ContentTypeName.Image) return array().min(1, 'Tone is a required field.');
      return array();
    }),
    // TODO: validation for print content.
    // section: string().when('contentType', (value: string[]) => {
    //   if (value[0] === ContentTypeName.PrintContent)
    //     return string().trim().required('Section is a required field.');
    //   return string();
    // }),
    // byline: string().when('contentType', (value: string[]) => {
    //   if (value[0] === ContentTypeName.PrintContent)
    //     return string().trim().required('Byline is a required field.');
    //   return string();
    // }),
  },
  [['sourceId', 'tempSource']],
);
