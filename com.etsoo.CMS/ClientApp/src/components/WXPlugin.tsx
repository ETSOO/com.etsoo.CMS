import { Switch, TextFieldEx, VBox } from '@etsoo/materialui';
import { DomUtils } from '@etsoo/shared';
import { app } from '../app/MyApp';
import { PluginItem, PluginProps } from './PluginItem';

/**
 * Wechat plugin
 * @returns Component
 */
export function WXPlugin(props: PluginProps) {
  const name = 'WX';
  const secret = '******';
  const { initData, disabled } = props;
  const item = initData?.find((d) => d.id === name);

  const labels = app.getLabels('serviceWXApp', 'serviceWXSecret', 'enabled');

  return (
    <PluginItem
      name={name}
      url="https://mp.weixin.qq.com/"
      initData={item}
      disabled={disabled}
      inputs={(data) => (
        <VBox gap={1} marginTop={1}>
          <TextFieldEx
            name="app"
            label={labels.serviceWXApp}
            autoCorrect="off"
            defaultValue={data?.app ?? 'wx'}
            showClear
            required
          />
          <TextFieldEx
            name="secret"
            label={labels.serviceWXSecret}
            autoCorrect="off"
            defaultValue={data?.app ? secret : ''}
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
          (data.secret !== secret && data.secret.length < 16)
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
