import { Switch, TextFieldEx, VBox } from '@etsoo/materialui';
import { DomUtils } from '@etsoo/shared';
import { app } from '../app/MyApp';
import { PluginItem, PluginProps } from './PluginItem';

/**
 * Baidu JinQiao
 * @returns Component
 */
export function BDJQPlugin(props: PluginProps) {
  const name = 'BDJQ';
  const { initData, disabled } = props;
  const item = initData?.find((d) => d.id === name);

  const labels = app.getLabels('serviceBDJQApp', 'enabled');

  return (
    <PluginItem
      name={name}
      url="https://aifanfan.baidu.com/"
      initData={item}
      disabled={disabled}
      inputs={(data) => (
        <VBox gap={1} marginTop={1}>
          <TextFieldEx
            name="app"
            label={labels.serviceBDJQApp}
            autoCorrect="off"
            defaultValue={data?.app ?? ''}
            showClear
            required
          />
          <Switch
            name="enabled"
            label={labels.enabled}
            checked={data?.enabled ?? true}
          />
        </VBox>
      )}
      validator={(form, data, _editing) => {
        if (data.app.length < 28) {
          DomUtils.setFocus('app', form);
          return false;
        }
      }}
    />
  );
}
