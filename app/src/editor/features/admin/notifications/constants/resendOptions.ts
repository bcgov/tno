import { OptionItem, ResendOptionName } from '@/shared';

export const resendOptions = [
  new OptionItem('Never resend, only alert the first time published', ResendOptionName.Never),
  new OptionItem('Resend every update', ResendOptionName.Updated),
  new OptionItem('Resend every time published', ResendOptionName.Republished),
  new OptionItem(
    'Resend every time published with approved transcript',
    ResendOptionName.Transcribed,
  ),
];
