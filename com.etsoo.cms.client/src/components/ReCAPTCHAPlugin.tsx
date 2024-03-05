import { Switch, TextFieldEx, VBox } from '@etsoo/materialui';
import { DomUtils } from '@etsoo/shared';
import { app } from '../app/MyApp';
import {
  checkSecret,
  loadPluginSecret,
  PluginItem,
  PluginProps
} from './PluginItem';

/**
 * reCAPTCHAPlugin plugin
 * https://developers.google.com/recaptcha/docs/v3
 * @returns Component
 */
export function ReCAPTCHAPlugin(props: PluginProps) {
  const name = 'RECAP';
  const secret = '******';
  const { initData, disabled } = props;
  const item = initData?.find((d) => d.id === name);
  const json = `{"baseAddress": "R", "secret": ""}`;

  const labels = app.getLabels(
    'serviceRECAPApp',
    'serviceRECAPSecret',
    'enabled'
  );

  return (
    <PluginItem
      name={name}
      url="https://developers.google.com/recaptcha/docs/v3"
      initData={item}
      disabled={disabled}
      inputs={(data) => (
        <VBox gap={1} marginTop={1}>
          <TextFieldEx
            name="app"
            label={labels.serviceRECAPApp}
            autoCorrect="off"
            defaultValue={data?.app ?? ''}
            showClear
            required
          />
          <TextFieldEx
            name="secret"
            label={labels.serviceRECAPSecret}
            multiline
            rows={2}
            autoCorrect="off"
            defaultValue={data?.app ? secret : json}
            onDoubleClick={() => navigator.clipboard.writeText(json)}
            id={item?.id}
            onVisibility={loadPluginSecret}
            showClear
            showPassword
            required
          />
          <Switch
            name="enabled"
            label={labels.enabled}
            checked={data?.enabled ?? true}
          />
        </VBox>
      )}
      validator={(form, data, editing) => {
        if (data.app.length < 16) {
          DomUtils.setFocus('app', form);
          return false;
        }

        if (
          data.secret == null ||
          (!editing && data.secret === secret) ||
          (data.secret !== secret && !checkSecret(data.secret))
        ) {
          DomUtils.setFocus('secret', form);
          return false;
        }

        if (editing && data.secret === secret) {
          data.secret = undefined;
        }
      }}
    />
  );
}
