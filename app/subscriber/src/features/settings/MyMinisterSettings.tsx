import { useLookup } from 'store/hooks';
import { IOptionItem, RadioGroup, Row } from 'tno-core';

import * as styled from './styled';
import React from 'react';

export const MyMinisterSettings: React.FC = () => {
  const [{ ministers }] = useLookup();
  const [myMinister, setMyMinister] = React.useState<string>();

  React.useEffect(() => {
    if (!!myMinister) localStorage.setItem('myMinister', myMinister);
  }, [myMinister]);

  const options = ministers.map((m) => {
    return {
      label: (
        <Row className="options">
          <b>{m.name} | </b>
          <span className="desc">{m.description}</span>
        </Row>
      ),
      value: m.name,
      isEnabled: true,
      discriminator: 'IOption',
    } as IOptionItem;
  });

  return (
    <styled.MyMinisterSettings>
      <p className="description">
        Choose the Minister you'd like to follow. Stories about your selected Minister will be
        available from a quick click in the sidebar menu.
      </p>
      <RadioGroup
        value={
          !!myMinister
            ? options.find((o) => o.value === myMinister)
            : options.find((o) => o.value === localStorage.getItem('myMinister'))
        }
        onChange={(e) => {
          setMyMinister(e.target.value);
        }}
        options={options}
        name="ministers"
      />
    </styled.MyMinisterSettings>
  );
};
