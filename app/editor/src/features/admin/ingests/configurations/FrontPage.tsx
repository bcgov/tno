import { useFormikContext } from 'formik';
import { useTooltips } from 'hooks';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';
import { FormikText } from 'tno-core';

import * as styled from './styled';

export const FrontPage: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();
  useTooltips();

  return (
    <styled.IngestType>
      <FormikText
        label="Path to Files"
        name="configuration.path"
        value={values.configuration.path}
      />
      <FormikText
        label="File Name Expression (i.e. Code)"
        name="configuration.fileName"
        value={values.configuration.fileName}
      />
      <p>
        Front pages are often delivered the evening before the publish date. Use a regular
        expression to parse the file name to determine the published on date. Use regex group names
        to identify each part of the date, (?&lt;year&gt;) (?&lt;month&gt;) (?&lt;day&gt;)
        (?&lt;hour&gt;) (?&lt;minute&gt;) (?&lt;second&gt;)
      </p>
      <FormikText
        label="Parse Published On from File Name"
        name="configuration.publishedOnExpression"
        tooltip="Use a regex to extract the published on date"
        placeholder="^sv-GLB-[A-Za-z]{3}-(?<day>[0-9]{2})(?<month>[0-9]{2})(?<year>[0-9]{4})"
        value={values.configuration.publishedOnExpression}
      />
    </styled.IngestType>
  );
};
